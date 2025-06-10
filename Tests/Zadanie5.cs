using Microsoft.VisualStudio.TestTools.UnitTesting;
using ver5;
using System;
using System.IO;

namespace ver5UnitTests
{
    public class ConsoleRedirectionToStringWriter : IDisposable
    {
        private StringWriter stringWriter;
        private TextWriter originalOutput;

        public ConsoleRedirectionToStringWriter()
        {
            stringWriter = new StringWriter();
            originalOutput = Console.Out;
            Console.SetOut(stringWriter);
        }

        public string GetOutput()
        {
            return stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(originalOutput);
            stringWriter.Dispose();
        }
    }

    [TestClass]
    public class UnitTestDevices
    {
        [TestMethod]
        public void Printer_Print_DeviceOn()
        {
            var printer = new Printer();
            printer.PowerOn();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                printer.Print(in doc);
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Print: test.pdf"));
            }
            Assert.AreEqual(1, printer.PrintCounter);
        }

        [TestMethod]
        public void Printer_Print_DeviceOff()
        {
            var printer = new Printer();
            printer.PowerOff();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                printer.Print(in doc);
                Assert.IsFalse(consoleOutput.GetOutput().Contains("Print"));
            }
            Assert.AreEqual(0, printer.PrintCounter);
        }

        [TestMethod]
        public void Scanner_Scan_DeviceOn()
        {
            var scanner = new Scanner();
            scanner.PowerOn();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc;
                scanner.Scan(out doc, IDocument.FormatType.PDF);
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Scan: PDFScan0.pdf"));
            }
            Assert.AreEqual(1, scanner.ScanCounter);
        }

        [TestMethod]
        public void Scanner_Scan_DeviceOff()
        {
            var scanner = new Scanner();
            scanner.PowerOff();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc;
                scanner.Scan(out doc, IDocument.FormatType.PDF);
                Assert.IsFalse(consoleOutput.GetOutput().Contains("Scan"));
            }
            Assert.AreEqual(0, scanner.ScanCounter);
        }

        [TestMethod]
        public void Fax_SendFax_DeviceOn()
        {
            var fax = new Fax();
            fax.PowerOn();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                fax.SendFax(in doc, "123456789");
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Send Fax: test.pdf to 123456789"));
            }
            Assert.AreEqual(1, fax.FaxCounter);
        }

        [TestMethod]
        public void Fax_SendFax_DeviceOff()
        {
            var fax = new Fax();
            fax.PowerOff();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                fax.SendFax(in doc, "123456789");
                Assert.IsFalse(consoleOutput.GetOutput().Contains("Send Fax"));
            }
            Assert.AreEqual(0, fax.FaxCounter);
        }

        [TestMethod]
        public void MultifunctionalDevice_Print_DeviceOn()
        {
            var device = new MultifunctionalDevice();
            device.PowerOn();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                device.Print(in doc);
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Print: test.pdf"));
            }
            Assert.AreEqual(1, device.PrintCounter);
        }

        [TestMethod]
        public void MultifunctionalDevice_ScanAndPrint_DeviceOn()
        {
            var device = new MultifunctionalDevice();
            device.PowerOn();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                device.ScanAndPrint();
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Scan"));
                Assert.IsTrue(consoleOutput.GetOutput().Contains("Print"));
            }
        }

        [TestMethod]
        public void MultifunctionalDevice_SendFax_DeviceOff()
        {
            var device = new MultifunctionalDevice();
            device.PowerOff();

            var currentConsoleOut = Console.Out;
            currentConsoleOut.Flush();
            using (var consoleOutput = new ConsoleRedirectionToStringWriter())
            {
                IDocument doc = new PDFDocument("test.pdf");
                device.SendFax(in doc, "123456789");
                Assert.IsFalse(consoleOutput.GetOutput().Contains("Send Fax"));
            }
            Assert.AreEqual(0, device.FaxCounter);
        }
    }
}