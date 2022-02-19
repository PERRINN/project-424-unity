using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public interface IAutopilotProvider : IReadOnlyList<Sample>
    {
        Sample this[Vector3 position] { get; }
    } 
}
