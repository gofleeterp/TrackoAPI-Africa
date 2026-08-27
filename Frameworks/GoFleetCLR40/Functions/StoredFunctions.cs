using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFleetCLR.Functions
{
    public partial class StoredFunctions
    {
        [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true, IsPrecise = false)]
        public static SqlDouble Levenshtein(SqlString stringOne, SqlString stringTwo)
        {
            #region Handle for Null value

            if (stringOne.IsNull)
                stringOne = new SqlString("");

            if (stringTwo.IsNull)
                stringTwo = new SqlString("");

            #endregion

            #region Convert to Uppercase

            string strOneUppercase = stringOne.Value.ToUpper();
            string strTwoUppercase = stringTwo.Value.ToUpper();

            #endregion

            #region Quick Check and quick match score

            int strOneLength = strOneUppercase.Length;
            int strTwoLength = strTwoUppercase.Length;

            int[,] dimention = new int[strOneLength + 1, strTwoLength + 1];
            int matchCost = 0;

            if (strOneLength + strTwoLength == 0)
            {
                return 100;
            }
            else if (strOneLength == 0)
            {
                return 0;
            }
            else if (strTwoLength == 0)
            {
                return 0;
            }

            #endregion

            #region Levenshtein Formula

            for (int i = 0; i <= strOneLength; i++)
                dimention[i, 0] = i;

            for (int j = 0; j <= strTwoLength; j++)
                dimention[0, j] = j;

            for (int i = 1; i <= strOneLength; i++)
            {
                for (int j = 1; j <= strTwoLength; j++)
                {
                    if (strOneUppercase[i - 1] == strTwoUppercase[j - 1])
                        matchCost = 0;
                    else
                        matchCost = 1;

                    dimention[i, j] = System.Math.Min(System.Math.Min(dimention[i - 1, j] + 1, dimention[i, j - 1] + 1), dimention[i - 1, j - 1] + matchCost);
                }
            }

            #endregion

            // Calculate Percentage of match
            double percentage = System.Math.Round((1.0 - ((double)dimention[strOneLength, strTwoLength] / (double)System.Math.Max(strOneLength, strTwoLength))) * 100.0, 2);

            return percentage;
        }
    }
}
