/* Copyright 2017 BusterWood

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. 
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    public static partial class Extensions
    {
        public static Dictionary<string, object> ToDictionary(this Row row) => row.ToDictionary(cv => cv.Name, cv => cv.Value);

        public static int IndexOf<T>(this T[] items, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in items) // foreach loop on array avoids array bounds checks
            {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }
    }
    
}