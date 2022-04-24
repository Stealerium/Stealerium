using System;
using System.IO;
using System.Text;

namespace Stealerium.Helpers
{
    public class SqLite
    {
        private readonly ulong _dbEncoding;
        private readonly byte[] _fileBytes;
        private readonly ulong _pageSize;
        private readonly byte[] _sqlDataTypeSize = {0, 1, 2, 3, 4, 6, 8, 8, 0, 0};
        private string[] _fieldNames;
        private SqliteMasterEntry[] _masterTableEntries;
        private TableEntry[] _tableEntries;

        public SqLite(string fileName)
        {
            _fileBytes = File.ReadAllBytes(fileName);
            _pageSize = ConvertToULong(16, 2);
            _dbEncoding = ConvertToULong(56, 4);
            ReadMasterTable(100L);
        }

        public string GetValue(int rowNum, int field)
        {
            try
            {
                if (rowNum >= _tableEntries.Length)
                    return null;
                return field >= _tableEntries[rowNum].Content.Length ? null : _tableEntries[rowNum].Content[field];
            }
            catch
            {
                return "";
            }
        }

        public int GetRowCount()
        {
            return _tableEntries.Length;
        }

        private bool ReadTableFromOffset(ulong offset)
        {
            try
            {
                switch (_fileBytes[offset])
                {
                    case 13:
                    {
                        var num1 = (uint) (ConvertToULong((int) offset + 3, 2) - 1UL);
                        var num2 = 0;
                        if (_tableEntries != null)
                        {
                            num2 = _tableEntries.Length;
                            Array.Resize(ref _tableEntries, _tableEntries.Length + (int) num1 + 1);
                        }
                        else
                        {
                            _tableEntries = new TableEntry[(int) num1 + 1];
                        }

                        for (uint index1 = 0; (int) index1 <= (int) num1; ++index1)
                        {
                            var num3 = ConvertToULong((int) offset + 8 + (int) index1 * 2, 2);
                            if ((long) offset != 100L)
                                num3 += offset;
                            var endIdx1 = Gvl((int) num3);
                            Cvl((int) num3, endIdx1);
                            var endIdx2 = Gvl((int) ((long) num3 + (endIdx1 - (long) num3) + 1L));
                            Cvl((int) ((long) num3 + (endIdx1 - (long) num3) + 1L), endIdx2);
                            var num4 = num3 + (ulong) (endIdx2 - (long) num3 + 1L);
                            var endIdx3 = Gvl((int) num4);
                            var endIdx4 = endIdx3;
                            var num5 = Cvl((int) num4, endIdx3);
                            RecordHeaderField[] array = null;
                            var num6 = (long) num4 - endIdx3 + 1L;
                            var index2 = 0;
                            while (num6 < num5)
                            {
                                Array.Resize(ref array, index2 + 1);
                                var startIdx = endIdx4 + 1;
                                endIdx4 = Gvl(startIdx);
                                array[index2].Type = Cvl(startIdx, endIdx4);
                                array[index2].Size = array[index2].Type <= 9L ? _sqlDataTypeSize[array[index2].Type] :
                                    !IsOdd(array[index2].Type) ? (array[index2].Type - 12L) / 2L :
                                    (array[index2].Type - 13L) / 2L;
                                num6 = num6 + (endIdx4 - startIdx) + 1L;
                                ++index2;
                            }

                            if (array == null) continue;
                            _tableEntries[num2 + (int) index1].Content = new string[array.Length];
                            var num7 = 0;
                            for (var index3 = 0; index3 <= array.Length - 1; ++index3)
                            {
                                if (array[index3].Type > 9L)
                                {
                                    if (!IsOdd(array[index3].Type))
                                        switch ((long) _dbEncoding)
                                        {
                                            case 1L:
                                                _tableEntries[num2 + (int) index1].Content[index3] =
                                                    Encoding.Default.GetString(_fileBytes,
                                                        (int) ((long) num4 + num5 + num7), (int) array[index3].Size);
                                                break;
                                            case 2L:
                                                _tableEntries[num2 + (int) index1].Content[index3] =
                                                    Encoding.Unicode.GetString(_fileBytes,
                                                        (int) ((long) num4 + num5 + num7), (int) array[index3].Size);
                                                break;
                                            case 3L:
                                                _tableEntries[num2 + (int) index1].Content[index3] =
                                                    Encoding.BigEndianUnicode.GetString(_fileBytes,
                                                        (int) ((long) num4 + num5 + num7), (int) array[index3].Size);
                                                break;
                                        }
                                    else
                                        _tableEntries[num2 + (int) index1].Content[index3] =
                                            Encoding.Default.GetString(_fileBytes, (int) ((long) num4 + num5 + num7),
                                                (int) array[index3].Size);
                                }
                                else
                                {
                                    _tableEntries[num2 + (int) index1].Content[index3] =
                                        Convert.ToString(ConvertToULong((int) ((long) num4 + num5 + num7),
                                            (int) array[index3].Size));
                                }

                                num7 += (int) array[index3].Size;
                            }
                        }

                        break;
                    }
                    case 5:
                    {
                        var num1 = (uint) (ConvertToULong((int) ((long) offset + 3L), 2) - 1UL);
                        for (uint index = 0; (int) index <= (int) num1; ++index)
                        {
                            var num2 = (uint) ConvertToULong((int) offset + 12 + (int) index * 2, 2);
                            ReadTableFromOffset((ConvertToULong((int) ((long) offset + num2), 4) - 1UL) * _pageSize);
                        }

                        ReadTableFromOffset((ConvertToULong((int) ((long) offset + 8L), 4) - 1UL) * _pageSize);
                        break;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReadMasterTable(long offset)
        {
            while (true)
            {
                switch (_fileBytes[offset])
                {
                    case 5:
                        var num1 = (uint) (ConvertToULong((int) offset + 3, 2) - 1UL);
                        for (var index = 0; index <= (int) num1; ++index)
                        {
                            var num2 = (uint) ConvertToULong((int) offset + 12 + index * 2, 2);
                            if (offset == 100L)
                                ReadMasterTable(((long) ConvertToULong((int) num2, 4) - 1L) * (long) _pageSize);
                            else
                                ReadMasterTable(((long) ConvertToULong((int) (offset + num2), 4) - 1L) *
                                                (long) _pageSize);
                        }

                        offset = ((long) ConvertToULong((int) offset + 8, 4) - 1L) * (long) _pageSize;
                        continue;
                    case 13:
                        var num3 = ConvertToULong((int) offset + 3, 2) - 1UL;
                        var num4 = 0;
                        if (_masterTableEntries != null)
                        {
                            num4 = _masterTableEntries.Length;
                            Array.Resize(ref _masterTableEntries, _masterTableEntries.Length + (int) num3 + 1);
                        }
                        else
                        {
                            _masterTableEntries = new SqliteMasterEntry[checked((ulong) unchecked((long) num3 + 1L))];
                        }

                        for (ulong index1 = 0; index1 <= num3; ++index1)
                        {
                            var num2 = ConvertToULong((int) offset + 8 + (int) index1 * 2, 2);
                            if (offset != 100L) num2 += (ulong) offset;
                            var endIdx1 = Gvl((int) num2);
                            Cvl((int) num2, endIdx1);
                            var endIdx2 = Gvl((int) ((long) num2 + (endIdx1 - (long) num2) + 1L));
                            Cvl((int) ((long) num2 + (endIdx1 - (long) num2) + 1L), endIdx2);
                            var num5 = num2 + (ulong) (endIdx2 - (long) num2 + 1L);
                            var endIdx3 = Gvl((int) num5);
                            var endIdx4 = endIdx3;
                            var num6 = Cvl((int) num5, endIdx3);
                            var numArray = new long[5];
                            for (var index2 = 0; index2 <= 4; ++index2)
                            {
                                var startIdx = endIdx4 + 1;
                                endIdx4 = Gvl(startIdx);
                                numArray[index2] = Cvl(startIdx, endIdx4);
                                numArray[index2] = numArray[index2] <= 9L ? _sqlDataTypeSize[numArray[index2]] :
                                    !IsOdd(numArray[index2]) ? (numArray[index2] - 12L) / 2L :
                                    (numArray[index2] - 13L) / 2L;
                            }

                            if ((long) _dbEncoding == 1L || (long) _dbEncoding == 2L)

                                switch ((long) _dbEncoding)
                                {
                                    case 1L:
                                        _masterTableEntries[num4 + (int) index1].ItemName =
                                            Encoding.Default.GetString(_fileBytes,
                                                (int) ((long) num5 + num6 + numArray[0]), (int) numArray[1]);
                                        break;
                                    case 2L:
                                        _masterTableEntries[num4 + (int) index1].ItemName =
                                            Encoding.Unicode.GetString(_fileBytes,
                                                (int) ((long) num5 + num6 + numArray[0]), (int) numArray[1]);
                                        break;
                                    case 3L:
                                        _masterTableEntries[num4 + (int) index1].ItemName =
                                            Encoding.BigEndianUnicode.GetString(_fileBytes,
                                                (int) ((long) num5 + num6 + numArray[0]), (int) numArray[1]);
                                        break;
                                }

                            _masterTableEntries[num4 + (int) index1].RootNum = (long) ConvertToULong(
                                (int) ((long) num5 + num6 + numArray[0] + numArray[1] + numArray[2]),
                                (int) numArray[3]);
                            switch ((long) _dbEncoding)
                            {
                                case 1L:
                                    _masterTableEntries[num4 + (int) index1].SqlStatement =
                                        Encoding.Default.GetString(_fileBytes,
                                            (int) ((long) num5 + num6 + numArray[0] + numArray[1] + numArray[2] +
                                                   numArray[3]), (int) numArray[4]);
                                    break;
                                case 2L:
                                    _masterTableEntries[num4 + (int) index1].SqlStatement =
                                        Encoding.Unicode.GetString(_fileBytes,
                                            (int) ((long) num5 + num6 + numArray[0] + numArray[1] + numArray[2] +
                                                   numArray[3]), (int) numArray[4]);
                                    break;
                                case 3L:
                                    _masterTableEntries[num4 + (int) index1].SqlStatement =
                                        Encoding.BigEndianUnicode.GetString(_fileBytes,
                                            (int) ((long) num5 + num6 + numArray[0] + numArray[1] + numArray[2] +
                                                   numArray[3]), (int) numArray[4]);
                                    break;
                            }
                        }

                        break;
                }

                break;
            }
        }

        public bool ReadTable(string tableName)
        {
            var index1 = -1;
            for (var index2 = 0; index2 <= _masterTableEntries.Length; ++index2)
                if (string.Compare(_masterTableEntries[index2].ItemName.ToLower(), tableName.ToLower(),
                        StringComparison.Ordinal) == 0)
                {
                    index1 = index2;
                    break;
                }

            if (index1 == -1)
                return false;
            var strArray = _masterTableEntries[index1].SqlStatement
                .Substring(_masterTableEntries[index1].SqlStatement.IndexOf("(", StringComparison.Ordinal) + 1)
                .Split(',');
            for (var index2 = 0; index2 <= strArray.Length - 1; ++index2)
            {
                strArray[index2] = strArray[index2].TrimStart();
                var length = strArray[index2].IndexOf(' ');
                if (length > 0)
                    strArray[index2] = strArray[index2].Substring(0, length);
                if (strArray[index2].IndexOf("UNIQUE", StringComparison.Ordinal) == 0) continue;
                Array.Resize(ref _fieldNames, index2 + 1);
                _fieldNames[index2] = strArray[index2];
            }

            return ReadTableFromOffset((ulong) (_masterTableEntries[index1].RootNum - 1L) * _pageSize);
        }

        private ulong ConvertToULong(int startIndex, int size)
        {
            try
            {
                if ((size > 8) | (size == 0))
                    return 0;
                ulong num = 0;
                for (var index = 0; index <= size - 1; ++index)
                    num = (num << 8) | _fileBytes[startIndex + index];
                return num;
            }
            catch
            {
                return 0;
            }
        }

        private int Gvl(int startIdx)
        {
            try
            {
                if (startIdx > _fileBytes.Length)
                    return 0;
                for (var index = startIdx; index <= startIdx + 8; ++index)
                {
                    if (index > _fileBytes.Length - 1)
                        return 0;
                    if ((_fileBytes[index] & 128) != 128)
                        return index;
                }

                return startIdx + 8;
            }
            catch
            {
                return 0;
            }
        }

        private long Cvl(int startIdx, int endIdx)
        {
            try
            {
                ++endIdx;
                var numArray = new byte[8];
                var num1 = endIdx - startIdx;
                var flag = false;
                if ((num1 == 0) | (num1 > 9))
                    return 0;
                switch (num1)
                {
                    case 1:
                        numArray[0] = (byte) (_fileBytes[startIdx] & (uint) sbyte.MaxValue);
                        return BitConverter.ToInt64(numArray, 0);
                    case 9:
                        flag = true;
                        break;
                }

                var num2 = 1;
                var num3 = 7;
                var index1 = 0;
                if (flag)
                {
                    numArray[0] = _fileBytes[endIdx - 1];
                    --endIdx;
                    index1 = 1;
                }

                var index2 = endIdx - 1;
                while (index2 >= startIdx)
                {
                    if (index2 - 1 >= startIdx)
                    {
                        numArray[index1] = (byte) (((_fileBytes[index2] >> (num2 - 1)) & (byte.MaxValue >> num2)) |
                                                   (_fileBytes[index2 - 1] << num3));
                        ++num2;
                        ++index1;
                        --num3;
                    }
                    else if (!flag)
                    {
                        numArray[index1] = (byte) ((_fileBytes[index2] >> (num2 - 1)) & (byte.MaxValue >> num2));
                    }

                    index2 += -1;
                }

                return BitConverter.ToInt64(numArray, 0);
            }
            catch
            {
                return 0;
            }
        }

        private static bool IsOdd(long value)
        {
            return (value & 1L) == 1L;
        }

        public static SqLite ReadTable(string database, string table)
        {
            if (!File.Exists(database))
                return null;
            var newPath = Path.GetTempFileName() + ".tmpdb";
            File.Copy(database, newPath);
            var sqLiteConnection = new SqLite(newPath);
            sqLiteConnection.ReadTable(table);
            File.Delete(newPath);
            return sqLiteConnection.GetRowCount() == 65536 ? null : sqLiteConnection;
        }

        private struct RecordHeaderField
        {
            public long Size;
            public long Type;
        }

        private struct TableEntry
        {
            public string[] Content;
        }

        private struct SqliteMasterEntry
        {
            public string ItemName;
            public long RootNum;
            public string SqlStatement;
        }
    }
}