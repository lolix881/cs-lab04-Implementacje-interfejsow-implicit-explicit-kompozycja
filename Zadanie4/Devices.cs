using System;
using System.Text.RegularExpressions;

namespace ver4
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

    public class Copier : IPrinter, IScanner
    {
        public int PrintCounter { get; private set; } = 0;
        public int ScanCounter { get; private set; } = 0;
        public int Counter { get; private set; } = 0;

        private IDevice.State deviceState = IDevice.State.off;
        private IDevice.State printerState = IDevice.State.standby;
        private IDevice.State scannerState = IDevice.State.standby;
        private int printBatch = 0;
        private int scanBatch = 0;

        public IDevice.State GetState() => deviceState;

        public void SetState(IDevice.State newState)
        {
            deviceState = newState;
            if (newState == IDevice.State.on)
            {
                printerState = IDevice.State.standby;
                scannerState = IDevice.State.standby;
            }
            else
            {
                printerState = newState;
                scannerState = newState;
            }
            Console.WriteLine($"Device state changed to: {deviceState}");
        }

        public void PowerOn()
        {
            if (deviceState != IDevice.State.on)
            {
                SetState(IDevice.State.on);
                Counter++;
            }
        }

        public void PowerOff()
        {
            SetState(IDevice.State.off);
        }

        public void StandbyOn()
        {
            SetState(IDevice.State.standby);
        }

        public void StandbyOff()
        {
            SetState(IDevice.State.on);
        }

        public void Print(in IDocument document)
        {
            if (deviceState == IDevice.State.on && document != null)
            {
                if (printerState == IDevice.State.standby)
                {
                    printerState = IDevice.State.on;
                    Console.WriteLine("Printer module waking up from standby...");
                }
                scannerState = IDevice.State.standby;

                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Print: {document.GetFileName()}");
                PrintCounter++;
                printBatch++;

                if (printBatch >= 3)
                {
                    printerState = IDevice.State.standby;
                    printBatch = 0;
                    Console.WriteLine("Printer module entering standby after 3 prints.");
                }
            }
        }

        public void Scan(out IDocument document, IDocument.FormatType formatType = IDocument.FormatType.JPG)
        {
            document = null;
            if (deviceState == IDevice.State.on)
            {
                if (scannerState == IDevice.State.standby)
                {
                    scannerState = IDevice.State.on;
                    Console.WriteLine("Scanner module waking up from standby...");
                }
                printerState = IDevice.State.standby;

                document = formatType switch
                {
                    IDocument.FormatType.PDF => new PDFDocument($"{formatType}Scan{ScanCounter}.pdf"),
                    IDocument.FormatType.TXT => new TextDocument($"{formatType}Scan{ScanCounter}.txt"),
                    _ => new ImageDocument($"{formatType}Scan{ScanCounter}.jpg"),
                };
                Console.WriteLine($"{DateTime.Now.ToString("pl-PL")} Scan: {document.GetFileName()}");
                ScanCounter++;
                scanBatch++;

                if (scanBatch >= 2)
                {
                    scannerState = IDevice.State.standby;
                    scanBatch = 0;
                    Console.WriteLine("Scanner module entering standby after 2 scans.");
                }
            }
        }

        public void ScanAndPrint()
        {
            if (deviceState == IDevice.State.on)
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
}
