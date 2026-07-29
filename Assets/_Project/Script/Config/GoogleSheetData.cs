using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard
{
    [Serializable]
    public sealed class GoogleSheetDataRow
    {
        public List<string> Cells = new();
    }

    public sealed class GoogleSheetData : ScriptableObject
    {
        public List<GoogleSheetDataRow> Rows = new();
    }
}
