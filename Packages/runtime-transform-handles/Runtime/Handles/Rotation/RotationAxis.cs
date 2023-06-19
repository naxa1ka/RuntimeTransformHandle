using System;
using UnityEngine;

namespace NativeRobotics.RuntimeTransformHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 20.10.2020
     */
    public class RotationAxis : HandleBase
    {
        private Mesh     _arcMesh;
        private Material _arcMaterial;
        private Vector3  _axis;
        private Vector3  _rotatedAxis;
        private Plane    _axisPlane;
        private Vector3  _tangent;
        private Vector3  _biTangent;
        
        private Quaternion _startRotation;

        public RotationAxis Initialize(RuntimeTransformHandle p_runtimeHandle, Vector3 p_axis, Color p_color)
        {
            _parentTransformHandle = p_runtimeHandle;
            _axis = p_axis;
            _defaultColor = p_color;
            
            InitializeMaterial();

            transform.SetParent(p_runtimeHandle.transform, false);

            GameObject o = new GameObject();
            o.transform.SetParent(transform, false);
            MeshRenderer mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            MeshFilter mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateTorus(2f, .02f, 32, 6);
            MeshCollider mc = o.AddComponent<MeshCollider>();
            mc.sharedMesh = MeshUtils.CreateTorus(2f, .1f, 32, 6);
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, _axis);
            return this;
        }
        
        protected override void InitializeMaterial()
        {
            _material = new Material(Shader.Find("sHTiF/AdvancedHandleShader"));
            _material.color = _defaultColor;
        }

        public void Update()
        {
            _material.SetVector("_CameraPosition", _parentTransformHandle.handleCamera.transform.position);
            _material.SetFloat("_CameraDistance",
                (_parentTransformHandle.handleCamera.transform.position - _parentTransformHandle.transform.position)
                .magnitude);
        }

        public override void Interact(Vector3 p_previousPosition)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!_axisPlane.Raycast(cameraRay, out float hitT))
            {
                base.Interact(p_previousPosition);
                return;
            }
            
            Vector3 hitPoint     = cameraRay.GetPoint(hitT);
            Vector3 hitDirection = (hitPoint - _parentTransformHandle.target.Position).normalized;
            float   x            = Vector3.Dot(hitDirection, _tangent);
            float   y            = Vector3.Dot(hitDirection, _biTangent);
            float   angleRadians = Mathf.Atan2(y, x);
            float   angleDegrees = angleRadians * Mathf.Rad2Deg;

            if (_parentTransformHandle.rotationSnap != 0)
            {
                angleDegrees = Mathf.Round(angleDegrees / _parentTransformHandle.rotationSnap) * _parentTransformHandle.rotationSnap;
                angleRadians = angleDegrees                                                    * Mathf.Deg2Rad;
            }

            if (_parentTransformHandle.space == HandleSpace.LOCAL)
            {
                _parentTransformHandle.target.LocalRotation = _startRotation * Quaternion.AngleAxis(angleDegrees, _axis);
            }
            else
            {
                Vector3 invertedRotatedAxis = Quaternion.Inverse(_startRotation) * _axis;
                _parentTransformHandle.target.Rotation = _startRotation * Quaternion.AngleAxis(angleDegrees, invertedRotatedAxis);
            }

            _arcMesh = MeshUtils.CreateArc(transform.position, _hitPoint, _rotatedAxis, 2, angleRadians, Mathf.Abs(Mathf.CeilToInt(angleDegrees)) + 1);
            DrawArc();

            base.Interact(p_previousPosition);
        }
        
        public override bool CanInteract(Vector3 p_hitPoint)
        {
            var cameraDistance = (_parentTransformHandle.transform.position - _parentTransformHandle.handleCamera.transform.position).magnitude;
            var pointDistance = (p_hitPoint - _parentTransformHandle.handleCamera.transform.position).magnitude;
            return pointDistance <= cameraDistance;
        }

        public override void StartInteraction(Vector3 p_hitPoint)
        {
            if (!CanInteract(p_hitPoint))
                return;
           
            
            base.StartInteraction(p_hitPoint);
            
            _startRotation = _parentTransformHandle.space == HandleSpace.LOCAL ? _parentTransformHandle.target.LocalRotation : _parentTransformHandle.target.Rotation;

            _arcMaterial = new Material(Shader.Find("sHTiF/HandleShader"));
            _arcMaterial.color = new Color(1,1,0,.4f);
            _arcMaterial.renderQueue = 5000;
            //_arcMesh.gameObject.SetActive(true);

            if (_parentTransformHandle.space == HandleSpace.LOCAL)
            {
                _rotatedAxis = _startRotation * _axis;
            }
            else
            {
                _rotatedAxis     = _axis;
            }

            _axisPlane = new Plane(_rotatedAxis, _parentTransformHandle.target.Position);

            Vector3 startHitPoint;
            Ray     cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (_axisPlane.Raycast(cameraRay, out float hitT))
            {
                startHitPoint = cameraRay.GetPoint(hitT);
            }
            else
            {
                startHitPoint = _axisPlane.ClosestPointOnPlane(p_hitPoint);
            }
            
            _tangent   = (startHitPoint - _parentTransformHandle.target.Position).normalized;
            _biTangent = Vector3.Cross(_rotatedAxis, _tangent);
        }
        
        public override void EndInteraction()
        {
            base.EndInteraction();
            //Destroy(_arcMesh.gameObject);
            delta = 0;
        }

        private void DrawArc()
        {
            // _arcMaterial.SetPass(0);
            // Graphics.DrawMeshNow(_arcMesh, Matrix4x4.identity);
            Graphics.DrawMesh(_arcMesh, Matrix4x4.identity, _arcMaterial, 0);

            // GameObject arc = new GameObject();
            // MeshRenderer mr = arc.AddComponent<MeshRenderer>();
            // mr.material = new Material(Shader.Find("sHTiF/HandleShader"));
            // mr.material.color = new Color(1,1,0,.5f);
            // _arcMesh = arc.AddComponent<MeshFilter>();
        }
    }
}