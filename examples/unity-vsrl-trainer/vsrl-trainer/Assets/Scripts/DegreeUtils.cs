/*

Copyright (C) 2020 IBM. All Rights Reserved.

See LICENSE.txt file in the root directory
of this source tree for licensing information.

*/

using UnityEngine;

namespace FoxChicken.Scripts
{
    public class DegreeUtils
    {
        public static Vector3 PolarToCartesian(Vector2 polar)
        {
            Vector3 origin = new Vector3(0, 0, polar.y);

            //build a quaternion using euler angles for lat,lon
            Quaternion rotation = Quaternion.Euler(0, polar.x, 0);

            //transform our reference vector by the rotation.
            Vector3 vector = rotation * origin;

            return vector;
        }
    }
}
