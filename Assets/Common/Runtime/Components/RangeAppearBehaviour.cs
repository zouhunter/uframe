using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/RangeAppearBehaviour")]
    public class RangeAppearBehaviour : MonoBehaviour
    {
        MaterialPropertyBlock props;
        [SerializeField]
        Transform target;
        [SerializeField]
        float appearSpeed = 10f;
        [SerializeField]
        float disappearSpeed = 5f;
        [SerializeField]
        public float radius = 12f;
        [SerializeField]
        public bool keep = false;

        public MeshRenderer[] objects;
        float[] values;
        int shaderID;
        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        public void Init()
        {
            objects = this.gameObject.GetComponentsInChildren<MeshRenderer>();

            props = new MaterialPropertyBlock();
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].SetPropertyBlock(props);
            }
            values = new float[objects.Length];
            shaderID = Shader.PropertyToID("_Moved");
        }


        // Update is called once per frame
        void Update()
        {
            if (target == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                Vector3 offset = objects[i].transform.position - target.position;
                float sqrLen = offset.sqrMagnitude;
                if (sqrLen < radius * radius)
                {
                    values[i] = Mathf.Lerp(values[i], 1, Time.deltaTime * appearSpeed);
                }
                else if (!keep)
                {
                    values[i] = Mathf.Lerp(values[i], 0, Time.deltaTime * disappearSpeed);
                }
                props.SetFloat(shaderID, values[i]);
                objects[i].SetPropertyBlock(props);
            }
        }
    }
}