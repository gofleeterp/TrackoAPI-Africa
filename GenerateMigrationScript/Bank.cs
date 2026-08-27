using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateMigrationScript
{
    public class Bank
    {
        public decimal Money { get; set; }//90
        public decimal Withdraw(decimal money)//60
        {
            if (money/*60*/ <= Money/*100*/)
            {
                Money = Money - money;
                return money;//60
            }
            return 0;
        }
        public void Deposit(decimal money)//50
        {
            Money = Money + money;
        }

    }
    public class LoanLender
    {
        public decimal Money { get; set; }
        public void ApplyForLoan(string panNo,decimal income)
        {
            if(CheckApplicablity(panNo))
            {
                if(income>50000)
                {
                    //Pay 10000
                }
                else if(income > 500000)
                {
                    //Pay 200000
                }
            }
            else
            {
                //No Loan
            }
        }
       
        private bool CheckApplicablity(string panNo)
        {
            int cibilScore = CheckCIBILScore(panNo);//700
            if (cibilScore < 600)
            {
                return false;
            }
            return true;
        }
        private int CheckCIBILScore(string panNo)
        {
            return 700;
        }
    }
}
