using System.Globalization;
using System.Text;
using CsvHelper;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using PortfolioTracker.Models;

namespace PortfolioTracker.Services;

public class ReportService
{
    public byte[] GenerateCsvReport(DashboardViewModel dashboard, List<Asset> assets)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        writer.WriteLine("Portfolio Report");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();

        // Summary section
        writer.WriteLine("PORTFOLIO SUMMARY");
        writer.WriteLine("Total Portfolio Value," + dashboard.TotalPortfolioValue.ToString("N2"));
        writer.WriteLine("Total Gain/Loss," + dashboard.TotalGainLoss.ToString("N2"));
        writer.WriteLine("Total Gain/Loss %," + dashboard.TotalGainLossPercentage.ToString("N2") + "%");
        writer.WriteLine("Total Assets," + dashboard.TotalAssets);
        writer.WriteLine();

        // Asset allocation
        writer.WriteLine("ASSET ALLOCATION");
        writer.WriteLine("Symbol,Name,Quantity,Value,Percentage");
        
        foreach (var allocation in dashboard.AssetAllocations)
        {
            var asset = assets.FirstOrDefault(a => a.Symbol == allocation.Symbol);
            writer.WriteLine($"{allocation.Symbol},{allocation.Name},{asset?.Quantity:N8},{allocation.Value:N2},{allocation.Percentage:N2}%");
        }

        writer.Flush();
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    public byte[] GeneratePdfReport(DashboardViewModel dashboard, List<Asset> assets)
    {
        using var memoryStream = new MemoryStream();
        var pdfWriter = new PdfWriter(memoryStream);
        var pdfDocument = new PdfDocument(pdfWriter);
        var document = new Document(pdfDocument);

        // Title
        document.Add(new Paragraph("Portfolio Report")
            .SetFontSize(24)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(10));

        // Generated date
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

        // Summary section
        document.Add(new Paragraph("Portfolio Summary")
            .SetFontSize(14)
            .SetBold()
            .SetMarginTop(10)
            .SetMarginBottom(10));

        var summaryTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));
        summaryTable.AddCell(CreateTableCell("Metric"));
        summaryTable.AddCell(CreateTableCell("Value"));
        
        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Portfolio Value")));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${dashboard.TotalPortfolioValue:N2}")));
        
        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Gain/Loss")));
        var gainLossText = $"${dashboard.TotalGainLoss:N2} ({dashboard.TotalGainLossPercentage:N2}%)";
        summaryTable.AddCell(new Cell().Add(new Paragraph(gainLossText)));
        
        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Assets")));
        summaryTable.AddCell(new Cell().Add(new Paragraph(dashboard.TotalAssets.ToString())));

        document.Add(summaryTable);

        // Asset allocation section
        document.Add(new Paragraph("Asset Allocation")
            .SetFontSize(14)
            .SetBold()
            .SetMarginTop(20)
            .SetMarginBottom(10));

        var assetsTable = new Table(5).SetWidth(UnitValue.CreatePercentValue(100));
        assetsTable.AddCell(CreateTableCell("Symbol"));
        assetsTable.AddCell(CreateTableCell("Name"));
        assetsTable.AddCell(CreateTableCell("Quantity"));
        assetsTable.AddCell(CreateTableCell("Value"));
        assetsTable.AddCell(CreateTableCell("Percentage"));

        foreach (var allocation in dashboard.AssetAllocations)
        {
            var asset = assets.FirstOrDefault(a => a.Symbol == allocation.Symbol);
            
            assetsTable.AddCell(new Cell().Add(new Paragraph(allocation.Symbol)));
            assetsTable.AddCell(new Cell().Add(new Paragraph(allocation.Name)));
            assetsTable.AddCell(new Cell().Add(new Paragraph($"{asset?.Quantity:N8}")));
            assetsTable.AddCell(new Cell().Add(new Paragraph($"${allocation.Value:N2}")));
            assetsTable.AddCell(new Cell().Add(new Paragraph($"{allocation.Percentage:N2}%")));
        }

        document.Add(assetsTable);

        document.Close();
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    private Cell CreateTableCell(string text)
    {
        return new Cell()
            .Add(new Paragraph(text).SetBold())
            .SetBackgroundColor(new iText.Kernel.Colors.DeviceRgb(220, 220, 220));
    }
}
