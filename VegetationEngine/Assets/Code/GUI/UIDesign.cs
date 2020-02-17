using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GUI {
    class UIDesign {
        public enum Elements {
            Default = 0,
            Highlighted = 1,
            Selected = 2,
            Disabled = 3
        }

        public static Color[] Buttons = new Color[4] {
            new Color(64.0f / 255.0f, 156.0f / 255.0f, 91.0f / 255.0f, 255.0f / 255.0f),
            new Color(192.0f / 255.0f, 192.0f / 255.0f, 192.0f / 255.0f, 255.0f / 255.0f),
            new Color(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f),
            new Color(131.0f / 255.0f, 131.0f / 255.0f, 131.0f / 255.0f, 116.0f / 255.0f)
        };
    }
}
