using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    public class TimeEffect : MonoBehaviour
    {
        public Light staffLight;
        
        Animation m_Animation;

        void Awake()
        {
            m_Animation = GetComponent<Animation>();

            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            if (staffLight != null)
                staffLight.enabled = true;

            if (m_Animation)
                m_Animation.Play();

            StartCoroutine(DisableAtEndOfAnimation());
        }

        IEnumerator DisableAtEndOfAnimation()
        {
            yield return new WaitForSeconds(m_Animation.clip.length);

            gameObject.SetActive(false);

            if (staffLight != null)
                staffLight.enabled = false;
        }
    } 
}
