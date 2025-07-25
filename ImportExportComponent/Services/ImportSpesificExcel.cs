//using ClosedXML.Excel;
//using ImportExportComponent.Dtos;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using SharedLibrary.Exceptions;
//using TableComponent.Extensions;

//namespace ImportExportComponent.Services
//{
//    public class ImportSpesificExcel(EfEntitySetProvider setProvider)
//    {
//        private readonly EfEntitySetProvider _setProvider = setProvider;
//        public async Task<BudgetSheetDto> ExtractBudceSheetAsync(IFormFile file)
//        {
//            using var stream = new MemoryStream();
//            await file.CopyToAsync(stream);

//            using var workbook = new XLWorkbook(stream);
//            var sheet = workbook.Worksheet(1);

//            var organizationName = sheet.Cell("B5").GetString();

//            var organization = await _context.Organizations
//                .Where(o => o.ShortName == organizationName)
//                .FirstOrDefaultAsync() ?? throw new NotFoundException("Təşkilat tapılmadı.");

//            var filters = new BudgetSheetDto
//            {
//                OrganizationName = organizationName,
//                OrganizationId = organization.Id,
//                StartDate = sheet.Cell("B6").GetString(),
//                EndDate = sheet.Cell("C6").GetString(),
//                CompileDate = sheet.Cell("B7").GetString(),
//                FunctionalClassificationCode = sheet.Cell("A17").GetString(),
//                AdministrativeClassificationCode = sheet.Cell("B17").GetString(),
//                Records = []
//            };

//            var rowStart = 23;
//            var rowEnd = sheet.LastRowUsed().RowNumber(); // Axirinci setire qeder gedir

//            for (int row = rowStart; row <= rowEnd; row++)
//            {
//                var indicator = sheet.Cell(row, 1).GetString();
//                var economicCode = sheet.Cell(row, 2).GetString();

//                decimal estimatedAmount = 0;
//                if (decimal.TryParse(sheet.Cell(row, 3).GetString(), out var tempEstimatedAmount))
//                {
//                    estimatedAmount = tempEstimatedAmount;
//                }

//                decimal financingAmount = 0;
//                if (decimal.TryParse(sheet.Cell(row, 4).GetString(), out var tempFinancingAmount))
//                {
//                    financingAmount = tempFinancingAmount;
//                }

//                decimal cashExecution = 0;
//                if (decimal.TryParse(sheet.Cell(row, 5).GetString(), out var tempCashExecution))
//                {
//                    cashExecution = tempCashExecution;
//                }

//                decimal actualExpense = 0;
//                if (decimal.TryParse(sheet.Cell(row, 6).GetString(), out var tempActualExpense))
//                {
//                    actualExpense = tempActualExpense;
//                }

//                if (!string.IsNullOrEmpty(indicator) && !string.IsNullOrEmpty(economicCode) && row > 22)
//                {
//                    filters.Records.Add(new BudgetRecord
//                    {
//                        Indicator = indicator,
//                        EconomicClassificationCode = economicCode,
//                        EstimatedAmount = estimatedAmount,
//                        FinancingAmount = financingAmount,
//                        CashExecution = cashExecution,
//                        ActualExpense = actualExpense
//                    });
//                }
//            }

//            // Xərclərin cəmi üçün hesablamalar (C97, D97, E97, F97)
//            decimal totalAmountC = sheet.Cell("C87").GetValue<decimal>() + sheet.Cell("C80").GetValue<decimal>() + sheet.Cell("C33").GetValue<decimal>() + sheet.Cell("C23").GetValue<decimal>();
//            decimal totalAmountD = sheet.Cell("D87").GetValue<decimal>() + sheet.Cell("D80").GetValue<decimal>() + sheet.Cell("D33").GetValue<decimal>() + sheet.Cell("D23").GetValue<decimal>();
//            decimal totalAmountE = sheet.Cell("E87").GetValue<decimal>() + sheet.Cell("E80").GetValue<decimal>() + sheet.Cell("E33").GetValue<decimal>() + sheet.Cell("E23").GetValue<decimal>();
//            decimal totalAmountF = sheet.Cell("F87").GetValue<decimal>() + sheet.Cell("F80").GetValue<decimal>() + sheet.Cell("F33").GetValue<decimal>() + sheet.Cell("F23").GetValue<decimal>();

//            filters.Records.Add(new BudgetRecord
//            {
//                Indicator = "Xərclərin cəmi",
//                EconomicClassificationCode = "",
//                EstimatedAmount = totalAmountC,
//                FinancingAmount = totalAmountD,
//                CashExecution = totalAmountE,
//                ActualExpense = totalAmountF
//            });

//            return filters;
//        }
//    }
//}