using System;
using System.Text.RegularExpressions;

namespace ver5
{
    public interface IDevice
    {
        enum State { on, off, standby };

        void PowerOn()
        {
            SetState(State.on);
        }

        void PowerOff()
        {
            SetState(State.off);
        }

        void StandbyOn()
        {
            SetState(State.standby);
        }

        void StandbyOff()
        {
            SetState(State.on);
        }

        protected abstract void SetState(State state);

        int Counter { get; }
    }

    public interface IPrinter : IDevice
    {
        // Dokument jest drukowany, jeśli urządzenie włączone. W przeciwnym przypadku nic się nie wykonuje obiekt typu IDocument, różny od null
        void Print(in IDocument document);
    }

    public interface IScanner : IDevice
    {
        // dokument jest skanowany, jeśli urządzenie włączone  w przeciwnym przypadku nic się dzieje
        void Scan(out IDocument document, IDocument.FormatType formatType);
    }

    public interface IFax : IDevice
    {
        void SendFax(in IDocument document, string faxNumber);
    }

    public class Printer : IPrinter
    {
        public int PrintCounter { get; private set; } = 0; // zwraca aktualną liczbę wydrukowanych dokumentów
        public int Counter { get; private set; } = 0; // zwraca liczbę uruchomień drukarki
        private IDevice.State state = IDevice.State.off;
        public IDevice.State GetState() => state;

        public void SetState(IDevice.State newState)
        {
            state = newState;
            Console.WriteLine($"Printer state changed to: {state}");
        }

        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                SetState(IDevice.State.on);
                Counter++;
            }
        }

        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                SetState(IDevice.State.off);
            }
        }

        public void Print(in IDocument document)
        {
            if (state == IDevice.State.on && document != null)
            {
                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Print: {document.GetFileName()}");
                PrintCounter++;
            }
        }
    }


    public class Scanner : IScanner
    {
        public int ScanCounter { get; private set; } = 0; // zwraca liczbę zeskanowanych dokumentów
        public int Counter { get; private set; } = 0; // zwraca liczbę uruchomień skanera
        private IDevice.State state = IDevice.State.off;
        public IDevice.State GetState() => state;

        public void SetState(IDevice.State newState)
        {
            state = newState;
            Console.WriteLine($"Scanner state changed to: {state}");
        }

        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                SetState(IDevice.State.on);
                Console.WriteLine("Device is on ...");
                Counter++;
            }
        }

        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                SetState(IDevice.State.off);
                Console.WriteLine("... Device is off !");
            }
        }

        public void Scan(out IDocument document, IDocument.FormatType formatType = IDocument.FormatType.JPG)
        {
            if (state == IDevice.State.on)
            {
                document = formatType switch
                {
                    IDocument.FormatType.PDF => new PDFDocument($"{formatType}Scan{ScanCounter}.pdf"),
                    IDocument.FormatType.TXT => new TextDocument($"{formatType}Scan{ScanCounter}.txt"),
                    _ => new ImageDocument($"{formatType}Scan{ScanCounter}.jpg"),
                };
                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Scan: {document.GetFileName()}");
                ScanCounter++;
            }
            else
            {
                document = null;
            }
        }
    }


    public class Fax : IFax
    {
        public int FaxCounter { get; private set; } = 0;
        public int Counter { get; private set; } = 0;
        private IDevice.State state = IDevice.State.off;
        public IDevice.State GetState() => state;

        public void SetState(IDevice.State newState)
        {
            state = newState;
            Console.WriteLine($"Fax state changed to: {state}");
        }

        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                SetState(IDevice.State.on);
                Counter++;
            }
        }

        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                SetState(IDevice.State.off);
            }
        }

        public void SendFax(in IDocument document, string faxNumber)
        {
            if (state == IDevice.State.on)
            {
                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Send Fax: {document.GetFileName()} to {faxNumber}");
                FaxCounter++;
            }
        }
    }

    public class MultifunctionalDevice : IPrinter, IScanner, IFax
    {
        private Printer printer = new Printer();
        private Scanner scanner = new Scanner();
        private Fax fax = new Fax();

        private int printJobCount = 0;
        private int scanJobCount = 0;
        private bool manualStandby = false;

        public int Counter => printer.Counter + scanner.Counter + fax.Counter;
        public int PrintCounter => printer.PrintCounter;
        public int ScanCounter => scanner.ScanCounter;
        public int FaxCounter => fax.FaxCounter;

        public IDevice.State GetState()
        {
            if (manualStandby)
                return IDevice.State.standby;
            if (printer.GetState() == IDevice.State.on && scanner.GetState() == IDevice.State.on && fax.GetState() == IDevice.State.on)
                return IDevice.State.on;
            return IDevice.State.off;
        }

        public void PowerOn()
        {
            printer.SetState(IDevice.State.on);
            scanner.SetState(IDevice.State.on);
            fax.SetState(IDevice.State.on);
            manualStandby = false;
        }

        public void PowerOff()
        {
            printer.SetState(IDevice.State.off);
            scanner.SetState(IDevice.State.off);
            fax.SetState(IDevice.State.off);
            manualStandby = false;
        }

        public void Print(in IDocument document)
        {
            if (manualStandby) return;
            // Skaner w standby podczas drukowania
            scanner.SetState(IDevice.State.standby);
            printer.SetState(IDevice.State.on);
            printer.Print(in document);
            printJobCount++;
            // Po 3 wydrukach drukarka w standby
            if (printJobCount % 3 == 0)
            {
                printer.SetState(IDevice.State.standby);
            }
        }

        public void Scan(out IDocument document, IDocument.FormatType formatType = IDocument.FormatType.JPG)
        {
            if (manualStandby)
            {
                document = null;
                return;
            }
            // Drukarka w standby podczas skanowania
            printer.SetState(IDevice.State.standby);
            scanner.SetState(IDevice.State.on);
            scanner.Scan(out document, formatType);
            scanJobCount++;
            // Po 2 skanach skaner w standby
            if (scanJobCount % 2 == 0)
            {
                scanner.SetState(IDevice.State.standby);
            }
        }

        public void SendFax(in IDocument document, string faxNumber)
        {
            if (GetState() == IDevice.State.on && manualStandby)
            {
                fax.SetState(IDevice.State.on);
                fax.SendFax(in document, faxNumber);
            }
        }


        public void StandbyOn()
        {
            manualStandby = true;
            printer.SetState(IDevice.State.standby);
            scanner.SetState(IDevice.State.standby);
            fax.SetState(IDevice.State.standby);
        }

        public void StandbyOff()
        {
            manualStandby = false;
            printer.SetState(IDevice.State.on);
            scanner.SetState(IDevice.State.on);
            fax.SetState(IDevice.State.on);
        }

        public void SetState(IDevice.State state)
        {
            if (state == IDevice.State.standby)
                manualStandby = true;
            else if (state == IDevice.State.on)
                manualStandby = false;
            printer.SetState(state);
            scanner.SetState(state);
            fax.SetState(state);
        }

        public void ScanAndPrint()
        {
            if (manualStandby) return;
            IDocument scannedDocument;
            scanner.Scan(out scannedDocument);
            if (scannedDocument != null)
            {
                printer.Print(in scannedDocument);
            }
        }
    }


}
