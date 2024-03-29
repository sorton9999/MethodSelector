﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MethodSelector;
using CommonClasses;
using System.Windows.Threading;

namespace MethodSelectorConsole
{
    public class Bank
    {
        private readonly Dictionary<string, Account> accounts = new Dictionary<string, Account>();
        private AccountDetailsListViewModel accountDetails = new AccountDetailsListViewModel();
        private float checkingInterest = 0.05F;
        private float savingsInterest = 0.03F;

        public const string InitialID = "12345";
        public string latestAcctId = InitialID;

        #region Constructor

        public Bank()
        {

        }

        #endregion

        #region Properties

        public Dictionary<string, Account> Accounts
        {
            get;
            private set;
        }

        public AccountDetailsListViewModel AccountDetailsList
        {
            get { return accountDetails; }
            private set
            {
                accountDetails = value;
            }
        }

        public float CheckingInterest
        {
            get { return checkingInterest; }
            private set { checkingInterest = value; }
        }

        public float SavingsInterest
        {
            get { return savingsInterest; }
            private set { savingsInterest = value; }
        }

        public Dispatcher Dispatcher
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The main banking actions done here.
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="name">The name associated with the account</param>
        /// <param name="action">The banking action to perform</param>
        /// <param name="amount">The dollar amount associated with the banking action</param>
        /// <param name="acctType">The account type, interest bearing or not</param>
        /// <param name="openAcct">Are we opening a new account?</param>
        /// <returns></returns>
        public float PerformAction(string id, string name, string action, float amount, AccountType acctType = AccountType.SIMPLE_CHECKING, bool openAcct = false)
        {
            Account account = null;
            float balance = 0;
            try
            {
                account = FindAccount(id);
            }
            catch (MethodSelector.AccountNotFoundException e)
            {
                Console.WriteLine(e.Message);
                if (openAcct)
                {
                    account = MakeAccount(name, id, amount, acctType);
                }
                else
                {
                    Console.WriteLine("<<< Do you need to open an account for [{0}]? >>>", name);
                    throw;
                }
            }
            catch (Exception e)
            {
                throw new BankingException(e.Message, e.InnerException);
            }
            if (account != null)
            {
                if (openAcct)
                {
                    return account.AccountBalance();
                }
                try
                {
                    balance = AccountActions(account, action, amount);
                }
                catch (Exception)
                {
                    throw;
                }
                UpdateDetails(account, balance);
            }
            return balance;
        }

        public AccountDetailsViewModel[] GetDetailsByName(string name)
        {
            AccountDetailsViewModel[] ret = null;
            var item = accountDetails.AccountDetailsList.ToLookup(x => x.AccountName == name);
            var t = item[true];
            if (t.Count() > 0)
            {
                ret = new AccountDetailsViewModel[t.Count()];
                foreach (var p in t.Select((x, i) => new { x,i }))
                {
                    ret[p.i] = p.x;
                }
            }
            return ret;
        }

        public AccountDetailsViewModel AccountDetailsByAccountId(string id)
        {
            AccountDetailsViewModel ret = null;
            var item = accountDetails.AccountDetailsList.ToLookup(x => x.AccountId == id);
            foreach (var p in item[true])
            {
                ret = p;
            }
            return ret;
        }

        public AccountDetailsViewModel GetDetailsByIndex(int idx)
        {
            AccountDetailsViewModel details = null;
            try
            {
                details = accountDetails.AccountDetailsList[idx];
            }
            catch (Exception e)
            {
                throw new BankingException("Details not found at IDX: " + idx);
            }
            return details;
        }

        public void DumpAccounts()
        {
            foreach (var acct in accounts.Select((x, i) => new { Value = x, Index = i }))
            {
                acct.Value.Value.PrintAccountDetails(acct.Index);
            }
        }

        public string GetNewAcctId()
        {
            string newId = latestAcctId;

            long id = Convert.ToInt64(newId);
            newId = (++id).ToString();
            latestAcctId = newId;

            return newId;
        }

        #endregion

        #region Private Methods

        private float AccountActions(Account account, string action, float amount)
        {
            try
            {
                if (action == "print")
                {
                    account.PrintAccountDetails(0);
                    return account.AccountBalance();
                }
                else
                {
                    return (account.AccountAction(action, amount));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
                //return account.AccountBalance();
            }
        }

        private void UpdateDetails(Account account, float balance)
        {
            var item = accountDetails.AccountDetailsList.ToLookup(x => x.AccountId == account.AccountDetails.AccountId);
            foreach (var p in item[true])
            {
                p.Balance = balance;
            }
            
        }

        private Account MakeAccount(string name, string id, float deposit, AccountType acctType)
        {
            Console.WriteLine("<<< Making an account for: [{0}] with initial deposit: [{1}] >>>", name, deposit);
            Account account = null;
            switch (acctType)
            {
                case AccountType.SIMPLE_CHECKING:
                    account = new SimpleAccount(name, id, deposit);
                    break;
                case AccountType.INTEREST_CHECKING:
                    account = new InterestAccount(name, id, deposit);
                    break;
                case AccountType.SAVINGS:
                    account = new SavingsAccount(name, id, deposit);
                    break;
                case AccountType.OTHER:
                    throw new BankingException("Account of type OTHER not supported.");
                default:
                    throw new IllegalOperationException("Unsupported account type detected.  Nothing done.");
            }

            //if (extended)
            //{
            //    account = new InterestAccount(name, id, deposit);
            //}
            //else
            //{
            //    account = new SimpleAccount(name, id, deposit);
            //}
            if (account != null)
            {
                try
                {
                    accounts.Add(id, account);

                    if (!Dispatcher.CheckAccess())
                    {
                        Dispatcher.Invoke(AccountDetailsListViewModel.addAction, account.AccountDetails);
                    }
                    else
                    {
                        AccountDetailsListViewModel.addAction.Invoke(account.AccountDetails);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    account = null;
                }
            }
            return account;
        }

        //internal float PerformExtendedAction(string name, string action, float amt, bool openAcct)
        //{
        //    return (PerformAction(name, action, amt, true, openAcct));
        //}

        private Account FindAccount(string id)
        {
            Account account = null;
            try
            {
                var acct = accounts.ToLookup(x => x.Key == id);
                foreach (var p in acct[true])
                {
                    account = p.Value;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("FindAccount: " + e.Message);
                throw new AccountNotFoundException("Account Not Found: [" + id + "]");
            }
            if (account == null)
            {
                throw new AccountNotFoundException("Account Not Found: [" + id + "]");
            }
            return account;
        }

        #endregion

    }
}
