using System;
using System.Text.RegularExpressions;

namespace ver2
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

    public class Copier: IPrinter, IScanner
    {
        public int PrintCounter { get; private set; } = 0; // zwraca aktualną liczbę wydrukowanych dokumentów
        public int ScanCounter { get; private set; } = 0; // zwraca liczbę zeskanowanych dokumentów
        public int Counter { get; private set; } = 0; // zwraca liczbę uruchomień kserokopiarki
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

        public void ScanAndPrint()
        {
            if (state == IDevice.State.on)
            {
                IDocument document;
                Scan(out document, IDocument.FormatType.JPG);
                if (document != null)
                {
                    Print(in document);
                }
            }
        }
    }

    public class MultifunctionalDevice : Copier, IFax
    {
        public int FaxCounter { get; private set; } = 0; // zwraca liczbę wysłanych faksów

        public void SendFax(in IDocument document, string faxNumber)
        {
            if (GetState() == IDevice.State.on)
            {
                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Send Fax: {document.GetFileName()} to {faxNumber}");
                FaxCounter++;
            }
        }
    }
}
