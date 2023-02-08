using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AthenaFramework
{
    public interface IColorSelector
    {
        public abstract Color PrimaryColor { get; set; }
        public abstract Color SecondaryColor { get; set; }
        public abstract bool UseSecondary { get; }
    }
}
