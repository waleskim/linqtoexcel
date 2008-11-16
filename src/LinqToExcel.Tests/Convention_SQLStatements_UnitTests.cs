﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using log4net;
using System.Reflection;
using log4net.Appender;
using log4net.Core;
using System.Data.OleDb;

namespace LinqToExcel.Tests
{
    [Author("Paul Yoder", "paulyoder@gmail.com")]
    [FixtureCategory("Unit")]
    [TestFixture]
    public class Convention_SQLStatements_UnitTests : SQLLogStatements_Helper
    {
        [TestFixtureSetUp]
        public void fs()
        {
            InstantiateLogger();
        }

        [SetUp]
        public void s()
        {
            ClearLogEvents();
        }

        [Test]
        public void select_all()
        {
            var companies = from c in ExcelRepository.GetSheet<Company>("")
                            select c;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            Assert.AreEqual("SELECT * FROM [Sheet1$]", GetSQLStatement());
        }

        [Test]
        public void where_equals()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                         where p.Name == "Paul"
                         select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} = ?)", GetSQLFieldName("Name"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("Paul", GetSQLParameters()[0]);
        }

        [Test]
        public void where_not_equal()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.Name != "Paul"
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} <> ?)", GetSQLFieldName("Name"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("Paul", GetSQLParameters()[0]);
        }

        [Test]
        public void where_greater_than()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount > 25
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} > ?)", GetSQLFieldName("EmployeeCount"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("25", GetSQLParameters()[0]);
        }

        [Test]
        public void where_greater_than_or_equal()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount >= 25
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} >= ?)", GetSQLFieldName("EmployeeCount"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("25", GetSQLParameters()[0]);
        }

        [Test]
        public void where_lesser_than()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount < 25
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} < ?)", GetSQLFieldName("EmployeeCount"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("25", GetSQLParameters()[0]);
        }

        [Test]
        public void where_lesser_than_or_equal()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount <= 25
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} <= ?)", GetSQLFieldName("EmployeeCount"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("25", GetSQLParameters()[0]);
        }

        [Test]
        public void where_and()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount > 5 && p.CEO == "Paul"
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where (({0} > ?) AND ({1} = ?))", 
                                                GetSQLFieldName("EmployeeCount"),
                                                GetSQLFieldName("CEO"));
            string[] parameters = GetSQLParameters();

            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("5", parameters[0]);
            Assert.AreEqual("Paul", parameters[1]);
        }

        [Test]
        public void where_or()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.EmployeeCount > 5 || p.CEO == "Paul"
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where (({0} > ?) OR ({1} = ?))",
                                                GetSQLFieldName("EmployeeCount"),
                                                GetSQLFieldName("CEO"));
            string[] parameters = GetSQLParameters();

            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("5", parameters[0]);
            Assert.AreEqual("Paul", parameters[1]);
        }

        [Test]
        public void local_field_used()
        {
            string desiredName = "Paul";
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.Name == desiredName
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} = ?)", GetSQLFieldName("Name"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("Paul", GetSQLParameters()[0]);
        }

        [Test]
        public void constructor_with_constant_value_arguments()
        {
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.StartDate == new DateTime(2008, 10, 9)
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} = ?)", GetSQLFieldName("StartDate"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("10/9/2008", GetSQLParameters()[0]);
        }

        [Test]
        public void constructor_with_field_value_arguments()
        {
            int year = 1876;
            int month = 6;
            int day = 25;
            var companies = from p in ExcelRepository.GetSheet<Company>("")
                            where p.StartDate == new DateTime(year, month, day)
                            select p;

            try { companies.GetEnumerator(); }
            catch (OleDbException) { }
            string expectedSql = string.Format("SELECT * FROM [Sheet1$] Where ({0} = ?)", GetSQLFieldName("StartDate"));
            Assert.AreEqual(expectedSql, GetSQLStatement());
            Assert.AreEqual("6/25/1876", GetSQLParameters()[0]);
        }
    }
}
