﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void GuessCodePage()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      CsvHelper.GuessCodePage(setting);
      Assert.AreEqual(1200, setting.CodePageId);

      var setting2 = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "UnicodeUTF16BE.txt")
      };
      CsvHelper.GuessCodePage(setting2);
      Assert.AreEqual(1201, setting2.CodePageId);

      var setting3 = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Test.csv")
      };
      CsvHelper.GuessCodePage(setting3);
      Assert.AreEqual(65001, setting3.CodePageId);
    }

    [TestMethod]
    public void GuessHasHeader()
    {
      Assert.IsTrue(CsvHelper.GuessHasHeader(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      }, CancellationToken.None), "BasicCSV.txt");

      Assert.IsFalse(CsvHelper.GuessHasHeader(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "txTranscripts.txt")
      }, CancellationToken.None), "txTranscripts.txt");
    }

    [TestMethod]
    public void GuessStartRow()
    {
      Assert.AreEqual(0, CsvHelper.GuessStartRow(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      }), "BasicCSV.txt");
    }

    [TestMethod]
    public void GuessDelimiter()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      Assert.AreEqual(",", CsvHelper.GuessDelimiter(setting));

      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "LateStartRow.txt"))
      {
        SkipRows = 10,
        CodePageId = 20127
      };
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual("|", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void HasUsedQualifier()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };

      CsvHelper.RefreshCsvFile(setting, new DummyProcessDisplay());
      Assert.AreEqual(1200, setting.CodePageId);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);
    }

    [TestMethod]
    public void FillGuessColumnFormatInvalidateColumnHeader()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"),
        HasFieldHeader = true
      };
      // setting.TreatTextNullAsNull = true;

      using (var test = setting.GetFileReader())
      {
        test.Open(false, CancellationToken.None);
        var list = new List<string>();
        CsvHelper.InvalidateColumnHeader(setting);
      }
    }

    [TestMethod]
    public void GetColumnHeaderFileEmpty()
    {
      var headers = CsvHelper.GetColumnHeader(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt"),
        HasFieldHeader = true
      }, false, true, null).ToArray();
      Assert.AreEqual(0, headers.Length);
    }

    [TestMethod]
    public void GetColumnHeaderHeadings()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.HasFieldHeader = true;
      var headers = CsvHelper.GetColumnHeader(setting, false, true, null).ToArray();
      Assert.AreEqual(6, headers.Length);
      Assert.AreEqual("ID", headers[0]);
      Assert.AreEqual("IsNativeLang", headers[5]);
    }

    [TestMethod]
    public void GetColumnHeaderNoHeaders()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "UnicodeUTF8.txt"),
        HasFieldHeader = false
      };

      Assert.AreEqual("Column1", CsvHelper.GetColumnHeader(setting, false, true, null).First());
    }

    [TestMethod]
    public void GuessDelimiterComma()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";

      Assert.AreEqual(",", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterPipe()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "DifferentColumnDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual("|", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterQualifier()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual(",", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterTAB()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "txTranscripts.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";
      Assert.AreEqual("TAB", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessStartRow0()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(0, CsvHelper.GuessStartRow(test));
    }

    [TestMethod]
    public void GuessStartRow12()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "SkippingEmptyRowsWithDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(12, CsvHelper.GuessStartRow(test));
    }

    [TestMethod]
    public void HasUsedQualifierFalse()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"),
        HasFieldHeader = true
      };

      Assert.IsFalse(CsvHelper.HasUsedQualifier(setting, CancellationToken.None));
    }

    [TestMethod]
    public void HasUsedQualifierTrue()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt")
      };
      Assert.IsTrue(CsvHelper.HasUsedQualifier(setting, CancellationToken.None));
    }

    [TestMethod]
    public void NewCsvFileGuessAllHeadings()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFile(display);
      }

      Assert.AreEqual(0, setting.SkipRows);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);
      Assert.AreEqual(1200, setting.CodePageId); //UTF16_LE
    }

    [TestMethod]
    public void NewCsvFileGuessAllTestEmpty()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFile(display);
      }

      Assert.AreEqual(0, setting.SkipRows);
    }

    [TestMethod]
    public void TestGuessStartRow()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "LateStartRow.txt"))
      {
        CodePageId = 20127
      };
      test.FileFormat.FieldDelimiter = "|";
      test.FileFormat.FieldQualifier = "\"";
      var rows = CsvHelper.GuessStartRow(test);
      Assert.AreEqual(10, rows);
    }

    [TestMethod]
    public void GetEmptyColumnHeaderTest()
    {
      var setting = new CsvFile();
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt");
      setting.HasFieldHeader = false;
      using (var disp = new DummyProcessDisplay())
      {
        Assert.IsTrue(CsvHelper.GetEmptyColumnHeader(setting, disp).IsEmpty());
        setting.HasFieldHeader = true;
        var res = CsvHelper.GetEmptyColumnHeader(setting, disp);

        Assert.IsFalse(res.IsEmpty());
        Assert.AreEqual("ID", res[0]);
      }
    }

    [TestMethod]
    public void GetColumnIndexTest()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt")
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.HasFieldHeader = true;
      Assert.AreEqual(0, CsvHelper.GetColumnIndex(setting, "ID", true));
      Assert.AreEqual(2, CsvHelper.GetColumnIndex(setting, "ExamDate", true));
    }

    [TestMethod]
    public void GetColumnIndexTestID()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt"),
        ID = "MyLittleTest"
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.HasFieldHeader = true;
      Assert.AreEqual(0, CsvHelper.GetColumnIndex(setting, "ID", true));
      Assert.AreEqual(2, CsvHelper.GetColumnIndex(setting, "ExamDate", false));
    }

    [TestMethod]
    public void GuessNotADelimitedFileTest()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt"))
      {
        CodePageId = 65001
      };
      Assert.IsFalse(CsvHelper.GuessNotADelimitedFile(test));

      ICsvFile test2 = new CsvFile(Path.Combine(m_ApplicationDirectory, "RowWithoutColumnDelimiter.txt"))
      {
        CodePageId = 65001
      };
      Assert.IsTrue(CsvHelper.GuessNotADelimitedFile(test2));
    }

    [TestMethod]
    public void GuessNewlineTest()
    {
      // Create a test file with LF Only
      var fileName = Path.Combine(m_ApplicationDirectory, "TestFileLF.txt");
      using (var file = new StreamWriter(File.Open(fileName, FileMode.CreateNew), System.Text.Encoding.GetEncoding(65001)))
      {
        string[] lines = { "ID\tTitle\t\"Object ID\"",
"12367890\t5 Overview\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe",
"3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e",
"3SOF\t3 Overview\taff9ed00-016e-4202-a3df-27a3ce443e80",
"3T1SA\t3 Phase 1\t\"8d527a23-2777-4754-a73d-029f67abe715\"",
"3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2",
"3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147",
"7S721A\t7 راز\t2b9d291f-ce76-4947-ae7b-fec3531d1766",
"#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1"
 };
        foreach (string line in lines)
        {
          // If the line doesn't contain the word 'Second', write the line to the file.
          if (!line.Contains("Second"))
          {
            file.Write(line);
            file.Write('\n');
          }
        }
      }

      var Test = new CsvFile(Path.Combine(m_ApplicationDirectory, fileName))
      {
        CodePageId = 65001
      };
      Test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual("LF", CsvHelper.GuessNewline(Test));
    }
  }
}