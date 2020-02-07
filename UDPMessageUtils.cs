using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace shvAlert
{

    class UDPMessageUtils
    {
        public int gIndex;
        public bool flgDebug = false;
        //------------------------------------------------------------------------------------------

        public int Unpack1int(byte[] bData, string VarName)
        {
            byte b = bData[gIndex];
            int retValue = Convert.ToInt32(b);
            gIndex = gIndex + 1;
            if(flgDebug) Debug.WriteLine("Unpack1int {0} {1} {2}", VarName, gIndex, retValue);
            return retValue;
        }

        public uint Unpack4uint(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 4).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            uint retValue = BitConverter.ToUInt32(b, 0);
            gIndex = gIndex + 4;
            if (flgDebug) Debug.WriteLine("Unpack4uint {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
            return retValue;
        }

        public int Unpack4int(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 4).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            int retValue = BitConverter.ToInt32(b, 0);
            gIndex = gIndex + 4;
            if (flgDebug) Debug.WriteLine("Unpack4int {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
            return retValue;
        }

        public UInt64 Unpack8uint(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 8).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            UInt64 retValue = BitConverter.ToUInt64(b, 0);
            gIndex = gIndex + 8;
            if (flgDebug) Debug.WriteLine("Unpack8uint {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
            return retValue;
        }

        public ulong Unpack8ulong(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 8).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            ulong retValue = BitConverter.ToUInt64(b, 0);
            gIndex = gIndex + 8;
            if (flgDebug) Debug.WriteLine("Unpack8ulong {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
            return retValue;
        }

        public float Unpack8float(byte[] bData, string VarName)
        {
            byte[] bb = bData.GetSegment(gIndex, 8).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bb);
            }

            double d = BitConverter.Int64BitsToDouble(BitConverter.ToInt64(bb, 0));
            if (flgDebug) Debug.WriteLine("double  = {0}", d);
            float retValue = (float)d;

            gIndex = gIndex + 8;
            if (flgDebug) Debug.WriteLine("Unpack8float {0} {1} {2} ", VarName, gIndex, retValue /*, BitConverter.ToString(b)*/);
            return retValue;
        }

        public string Unpackstring(byte[] bData, string VarName)
        {
            int iNum = Unpack4int(bData,"string Num");

            if (0 < iNum)
            {
                byte[] b = bData.GetSegment(gIndex, (int)iNum).ToArray();

                if (BitConverter.IsLittleEndian)
                {
                    //Array.Reverse(b);
                }
                string retValue = Encoding.UTF8.GetString(b);
                gIndex = gIndex + retValue.Length;
                if (flgDebug) Debug.WriteLine("Unpackstring {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
                return retValue;
            }
            else
            {
                if (flgDebug) Debug.WriteLine("Unpackstring {0} {1} ", VarName, gIndex);              
            }
            return "";
        }

        public bool Unpackbool(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 1).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            bool retValue = BitConverter.ToBoolean(b, 0);
            gIndex = gIndex + 1;
            if (flgDebug) Debug.WriteLine("Unpackbool {0} {1} {2} {3}", VarName, gIndex, retValue, BitConverter.ToString(b));
            return retValue;
        }

        public DateTime UnpackDateTime(byte[] bData, string VarName)
        {
            byte[] b = bData.GetSegment(gIndex, 4).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            int mill = BitConverter.ToInt32(b, 0);

            DateTime t = DateTime.UtcNow;
            DateTime date1 = new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
            DateTime date2 = date1.AddMilliseconds(mill);

            gIndex = gIndex + 4;
            if (flgDebug) Debug.WriteLine("UnpackDateTime {0} {1} {2} {3}", gIndex, date2.ToLocalTime(),date2.ToLongTimeString(), BitConverter.ToString(b));
            return date2;
        }
    }
}