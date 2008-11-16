﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using System.IO;

namespace LinqToExcel.Tests
{
    [Author("Paul Yoder", "paulyoder@gmail.com")]
    [FixtureCategory("Integration")]
    [TestFixture]
    public class ConfiguredWorksheetName_IntegrationTests
    {
        private string _excelFileName;

        [TestFixtureSetUp]
        public void fs()
        {
            string testDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string excelFilesDirectory = Path.Combine(testDirectory, "ExcelFiles");
            _excelFileName = Path.Combine(excelFilesDirectory, "Companies.xls");
        }

        [Test]
        public void data_is_read_from_correct_worksheet()
        {
            var companies = from c in ExcelRepository.GetSheet<Company>(_excelFileName, "More Companies")
                            select c;

            Assert.AreEqual(3, companies.ToList().Count);
        }
    }
}
