using System;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace ver3
{
    public interface IDevice
    {
        enum State {on, off};

        void PowerOn(); // uruchamia urządzenie, zmienia stan na on
        void PowerOff(); // wyłącza urządzenie, zmienia stan na off
        State GetState(); // zwraca aktualny stan urządzenia

        int Counter {get;}  // zwraca liczbę charakteryzującą eksploatację urządzenia, np. liczbę uruchomień, liczbę wydrukow, liczbę skanów, ...
    }

    public interface IFax : IDevice
    {
        // dokument jest wysyłany, jeśli urządzenie włączone w przeciwnym przypadku nic się dzieje
        void SendFax(in IDocument document, string faxNumber);
    }

    public abstract class BaseDevice : IDevice
    {
        protected IDevice.State state = IDevice.State.off;
        public IDevice.State GetState() => state;

        public void PowerOff()
        {
            state = IDevice.State.off;
            Console.WriteLine("... Device is off !");
        }

        public void PowerOn()
        {
            state = IDevice.State.on;
            Console.WriteLine("Device is on ...");  
        }

        public int Counter { get; private set; } = 0;
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

    public class Printer: IPrinter
    {
        public int PrintCounter { get; private set; } = 0; // zwraca aktualną liczbę wydrukowanych dokumentów
        public int Counter { get; private set; } = 0; // zwraca liczbę uruchomień drukarki
        private IDevice.State state = IDevice.State.off;
        public IDevice.State GetState() => state;
        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                state = IDevice.State.on;
                Counter++;
                Console.WriteLine("Device is on ...");
            }
        }

        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                state = IDevice.State.off;
                Console.WriteLine("... Device is off !");
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
        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                state = IDevice.State.on;
                Console.WriteLine("Device is on ...");
                Counter++;
            }
        }
        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                state = IDevice.State.off;
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
        public int FaxCounter { get; private set; } = 0; // zwraca liczbę wysłanych faksów
        public int Counter { get; private set; } = 0; // zwraca liczbę uruchomień faksu
        private IDevice.State state = IDevice.State.off;

        public IDevice.State GetState() => state;
        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                state = IDevice.State.on;
                Counter++;
                Console.WriteLine("Fax is on ...");
            }
        }
        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                state = IDevice.State.off;
                Console.WriteLine("... Fax is off !");
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


    public class Copier : IDevice
    {
        private readonly IPrinter printer = new Printer();
        private readonly IScanner scanner = new Scanner();
        private IDevice.State state = IDevice.State.off;
        public int Counter { get; private set; } = 0; // liczba uruchomień kserokopiarki

        public int PrintCounter => (printer as Printer)?.PrintCounter ?? 0;
        public int ScanCounter => (scanner as Scanner)?.ScanCounter ?? 0;

        public IDevice.State GetState() => state;

        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                state = IDevice.State.on;
                Counter++;
                Console.WriteLine("Copier is on ...");
            }
        }

        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                state = IDevice.State.off;
                Console.WriteLine("... Copier is off !");
                printer.PowerOff();
                scanner.PowerOff();
            }
        }

        public void Scan(out IDocument document, IDocument.FormatType formatType = IDocument.FormatType.JPG)
        {
            document = null;
            if (state == IDevice.State.on)
            {
                scanner.PowerOn();
                scanner.Scan(out document, formatType);
                scanner.PowerOff();
            }
        }

        public void Print(in IDocument document)
        {
            if (state == IDevice.State.on && document != null)
            {
                printer.PowerOn();
                printer.Print(document);
                printer.PowerOff();
            }
        }

        public void ScanAndPrint()
        {
            if (state == IDevice.State.on)
            {
                scanner.PowerOn();
                printer.PowerOn();

                scanner.Scan(out IDocument doc, IDocument.FormatType.JPG);
                if (doc != null)
                {
                    printer.Print(doc);
                }

                printer.PowerOff();
                scanner.PowerOff();
            }
        }
    }
    // class Copier + class Fax
    public class MultifunctionalDevice : IDevice
    {
        private readonly Copier copier = new Copier();
        private readonly Fax fax = new Fax();
        private IDevice.State state = IDevice.State.off;
        public int Counter { get; private set; } = 0; // liczba uruchomień urządzenia
        public int PrintCounter => copier.PrintCounter; // liczba wydruków
        public int ScanCounter => copier.ScanCounter; // liczba skanów
        public int FaxCounter => fax.FaxCounter; // liczba wysłanych faksów
        public IDevice.State GetState() => state;
        public void PowerOn()
        {
            if (state == IDevice.State.off)
            {
                state = IDevice.State.on;
                copier.PowerOn();
                fax.PowerOn();
                Counter++;
                Console.WriteLine("Multifunctional device is on ...");
            }
        }
        public void PowerOff()
        {
            if (state == IDevice.State.on)
            {
                state = IDevice.State.off;
                copier.PowerOff();
                fax.PowerOff();
                Console.WriteLine("... Multifunctional device is off !");
            }
        }
        public void SendFax(in IDocument document, string faxNumber)
        {
            if (state == IDevice.State.on)
            {
                fax.SendFax(in document, faxNumber);
            }
        }
        public void Print(in IDocument document) => copier.Print(document);
        public void Scan(out IDocument document, IDocument.FormatType formatType = IDocument.FormatType.JPG) => copier.Scan(out document, formatType);
        public void ScanAndPrint() => copier.ScanAndPrint();
    }
}
