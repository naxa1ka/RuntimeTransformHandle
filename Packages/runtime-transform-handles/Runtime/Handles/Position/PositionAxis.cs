using System;
using UnityEngine;

namespace Shtif.RuntimeTransformHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 20.10.2020
     */
    public class PositionAxis : HandleBase
    {
        protected Vector3 _startPosition;
        protected Vector3 _axis;

        private Vector3 _interactionOffset;
        private Ray     _raxisRay;

        public PositionAxis Initialize(RuntimeTransformHandle p_runtimeHandle, Vector3 p_axis, Color p_color)
        {
            _parentTransformHandle = p_runtimeHandle;
            _axis = p_axis;
            _defaultColor = p_color;
            
            InitializeMaterial();

            transform.SetParent(p_runtimeHandle.transform, false);

            var o = _parentTransformHandle.CreateGameObject();
            o.transform.SetParent(transform, false);
            var mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            var mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(2f, .02f, .02f, 8, 1);
            var mc = o.AddComponent<MeshCollider>();
            mc.sharedMesh = MeshUtils.CreateCone(2f, .1f, .02f, 8, 1);
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, p_axis);

            o = _parentTransformHandle.CreateGameObject();
            o.transform.SetParent(transform, false);
            mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(.4f, .2f, .0f, 8, 1);
            mc = o.AddComponent<MeshCollider>();
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, _axis);
            o.transform.localPosition = p_axis * 2;

            return this;
        }

        public override void Interact(Vector3 p_previousPosition)
        {
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            var   closestT = HandleMathUtils.ClosestPointOnRay(_raxisRay, cameraRay);
            var hitPoint = _raxisRay.GetPoint(closestT);
            
            var offset = hitPoint + _interactionOffset - _startPosition;
            
            var snapping = _parentTransformHandle.positionSnap;
            var   snap     = Vector3.Scale(snapping, _axis).magnitude;
            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.RELATIVE)
            {
                offset = (Mathf.Round(offset.magnitude / snap) * snap) * offset.normalized; 
            }

            var position = _startPosition + offset;
            
            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.ABSOLUTE)
            {
                if (snapping.x != 0) position.x = Mathf.Round(position.x / snapping.x) * snapping.x;
                if (snapping.y != 0) position.y = Mathf.Round(position.y / snapping.y) * snapping.y;
                if (snapping.x != 0) position.z = Mathf.Round(position.z / snapping.z) * snapping.z;
            }
            
            _parentTransformHandle.TargetPosition.Position = position;

            base.Interact(p_previousPosition);
        }
        
        public override void StartInteraction(Vector3 p_hitPoint)
        {
            base.StartInteraction(p_hitPoint);
            
            _startPosition = _parentTransformHandle.TargetPosition.Position;

            var raxis = _parentTransformHandle.Space == HandleSpace.LOCAL
                ? _parentTransformHandle.TargetRotation.Rotation * _axis
                : _axis;
            
            _raxisRay = new Ray(_startPosition, raxis);

            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            var closestT = HandleMathUtils.ClosestPointOnRay(_raxisRay, cameraRay);
            var hitPoint = _raxisRay.GetPoint(closestT);
            
            _interactionOffset = _startPosition - hitPoint;
        }
    }
}