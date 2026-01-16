using System.Collections;
using UnityEngine;

namespace Services.CoroutineServices
{
    public interface ICoroutineService
    {
        Coroutine StartCoroutine(IEnumerator routine);
    }
}