/***************************************************************************************
 * CREATS A STATIC INSTANCE OF THE SCRIPT USED WITH THIS SIGLETON. THIS SCRIPT IS A GENERIC
 * SCRIPT OF MONOBEHAVIOUR TYPE.
 ***************************************************************************************/

using UnityEngine;

namespace XAYA
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        GameObject g = new GameObject(typeof(T).Name);
                        instance = g.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}