using UnityEngine;

namespace Aatrox
{
    public class GenericStyleController : MonoBehaviour
    {
        public StyleMeter styleMeter;

        public void AddStyle(float coefficient)
        {
            if (styleMeter) styleMeter.AddStyle(coefficient);
        }
    }
}
