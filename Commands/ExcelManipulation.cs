﻿using CentralAptitudeTest.Models;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;   // 사용한 엑셀 객체들을 해제 해주기 위한 참조
using Microsoft.Office.Interop.Excel;   // 액셀 사용을 위한 참조

namespace CentralAptitudeTest.Commands
{
    class ExcelManipulation
    {
        // OpenFile() 에서 사용
        private Config Config;

        private Application application;

        private Workbook InputDataWorkbook;
        private Workbook InputCollegeWorkbook;
        private Workbook OutputAllWorkbook;
        private Workbook OutputGraphWorkbook;

        private Worksheet InputDataWorksheet;
        private Worksheet InputCollegeWorksheet;
        private Worksheet OutputAllWorksheet;
        private Worksheet OutputGraphWorksheet;

        string DesktopPath;
        string Datetime = DateTime.Now.ToString("hhmmss");

        private Range CollegeListRange;
        private Range WholeInputDataRange;

        // ReadCollege() 에서 사용
        private List<string> CollegeList = new List<string>();
        private List<string> DepartList = new List<string>();
        private Dictionary<string, List<string>> ClassData = new Dictionary<string, List<string>>();

        public ExcelManipulation(Config config)
        {
            Debug.WriteLine("=============================생성자 동작 시작=============================");

            // 바탕화면 경로 불러오기
            DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Excel 파일 저장 경로 및 파일 이름 설정
            // string 내부에서  {0}을 통한 combine 실패. 해당 코드 사용 불가!!!
            // path = Path.Combine(DesktopPath, "{0}.xlsx", Datetime);

            Config = config;
            application = new Application();

            OpenFile(config);
        }


        public void OpenFile(Config config)
        {
            Debug.WriteLine("=============================파일 열기 시작=============================");
            // 입력 Excel 파일(워크북) 불러오기
            InputDataWorkbook = application.Workbooks.Open(config.FilePath.whole_data_filePath);
            InputCollegeWorkbook = application.Workbooks.Open(config.FilePath.process_data_filePath);

            Debug.WriteLine("입력 데이터 활성화 worksheet : " + InputDataWorkbook.Worksheets.Count);

            // Excel 화면 창 띄우기
            // application.Visible = true;

            if (config.FilePath.whole_data_filePath.Contains("대구가톨릭"))
            {
                OutputAllWorkbook = application.Workbooks.Open(@"C:\\code\\대구가톨릭대학교전체정리.xlsx");
                OutputGraphWorkbook = application.Workbooks.Open(@"C:\\code\\대구가톨릭대학그래프정리.xlsx");
            }
            else
            {
                OutputAllWorkbook = application.Workbooks.Add();
                OutputGraphWorkbook = application.Workbooks.Add();
            }            
            // Test용
            // 기존 Excel 파일(워크북) 불러오기
            //OutputAllWorkbook = application.Workbooks.Open(@"C:\\code\\대구가톨릭대학교전체정리.xlsx");
            //OutputGraphWorkbook = application.Workbooks.Open(@"C:\\code\\대구가톨릭대학그래프정리.xlsx");         

            // worksheet 생성하기
            InputDataWorksheet = (Worksheet)InputDataWorkbook.Sheets[1];
            InputCollegeWorksheet = (Worksheet)InputCollegeWorkbook.Sheets[1];
            OutputAllWorksheet = (Worksheet)OutputAllWorkbook.ActiveSheet;
            OutputGraphWorksheet = (Worksheet)OutputGraphWorkbook.ActiveSheet;

            // 전체 입력 data 영역 설정
            WholeInputDataRange = InputDataWorksheet.UsedRange;

            // 단과 대학 영역 설정
            CollegeListRange = InputCollegeWorksheet.UsedRange;
        }

        public void CloseFile()
        {
            Debug.WriteLine("=============================작업 완료, 파일 닫기 시작=============================");

            // Save -> Close 순으로 수행
            string allfilenaming = "전체" + Datetime + ".xlsx";
            string graphfilenaming = "그래프" + Datetime + ".xlsx";

            string allpath = Path.Combine(DesktopPath, allfilenaming);
            string graphpath = Path.Combine(DesktopPath, graphfilenaming);

            // Save -> SaveAs 순으로 수행
            // InputDataWorkbook.Save();
            OutputAllWorkbook.SaveAs(Filename: allpath);
            OutputGraphWorkbook.SaveAs(Filename: graphpath);

            InputDataWorkbook.Close();
            InputCollegeWorkbook.Close();
            OutputAllWorkbook.Close();
            OutputGraphWorkbook.Close();

            application.Quit();

            // background에서 실행중인 객체들 마저 확실하게 해제시켜주기 위하여 사용.
            Marshal.ReleaseComObject(InputDataWorksheet);
            Marshal.ReleaseComObject(InputCollegeWorksheet);
            Marshal.ReleaseComObject(OutputAllWorksheet);
            Marshal.ReleaseComObject(OutputGraphWorksheet);

            Marshal.ReleaseComObject(InputDataWorkbook);
            Marshal.ReleaseComObject(InputCollegeWorkbook);
            Marshal.ReleaseComObject(OutputAllWorkbook);
            Marshal.ReleaseComObject(OutputGraphWorkbook);

            Marshal.ReleaseComObject(application);

            GC.Collect();
        }

        // summary
        // 2번째 입력파일에서 부터 각 "단대"와 "학과"를 읽어오기
        // 읽어온 데이터로, 전체 데이터 "워크시트" 생성하기
        public void ReadCollege()
        {
            Debug.WriteLine("=============================단과대학 및 학과 읽기 시작=============================");

            var tempDepart = "";
            var tempCollege = "";
            var tempInputKey = new List<int>();
            var tempDepartInput = new List<string>();
            var CollegeRow = CollegeListRange.Rows.Count;
            var CollegeColumn = CollegeListRange.Columns.Count;

            // 첫째줄 제목을 지우기 위해 row=2부터 시작
            for(int row = 2; row < CollegeRow; row++)
            {
                tempDepart = (string)(CollegeListRange.Cells[row, 2] as Range).Value2;
                tempCollege = (string)(CollegeListRange.Cells[row, 1] as Range).Value2;

                CollegeList.Add(tempCollege);
                Debug.WriteLine("단과대 : " + tempCollege);
                // Range collegeinput = (Range)OutputAllWorksheet.Cells[row, 1];
                // collegeinput.Value = (string)(CollegeListRange.Cells[row, 1] as Range).Value2;

                if(tempCollege != null)
                {
                    tempInputKey.Add(row-2);
                    Debug.WriteLine("입력 row 수 : {0}", row);
                }

                DepartList.Add(tempDepart);
                Debug.WriteLine("학과 : " + tempDepart);
                // Range departinput = (Range)OutputAllWorksheet.Cells[row, 2];
                // departinput.Value = (string)(CollegeListRange.Cells[row, 2] as Range).Value2;                
            }

            // 마지막 row 값 넣기
            tempInputKey.Add(CollegeRow-2);

            for (int tempInputKeyIndex = 0; tempInputKeyIndex < tempInputKey.Count-1; tempInputKeyIndex++)
            {
                // 초기화
                tempDepartInput.Clear();
                try
                {
                    // 넣기 전에 임시 리스트 생성
                    for (int getpoint = tempInputKey[tempInputKeyIndex]; getpoint < tempInputKey[tempInputKeyIndex + 1]; getpoint++)
                    {
                        tempDepartInput.Add(DepartList[getpoint]);
                    }

                    // 딕셔너리에 삽입
                    ClassData.Add(CollegeList[tempInputKey[tempInputKeyIndex]], tempDepartInput);
                    Debug.WriteLine("{0} 딕셔너리 생성 완료!!!", CollegeList[tempInputKey[tempInputKeyIndex]]);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.WriteLine("딕셔너리 생성 종료");
                }
            }

            // CollegeList 에서 null값 전부 제거
            CollegeList.RemoveAll(item => item == null);

            CollegeList.ForEach(CollegeList => Debug.WriteLine(CollegeList));
            DepartList.ForEach(DepartList => Debug.WriteLine(DepartList));

            // ClassData Dictionary 검사 부분
            foreach(KeyValuePair<string, List<string>> items in ClassData)
            {
                Debug.WriteLine("{0}. {1}", items.Key, items.Value);
            }

            // 결과 엑셀에 학과별 worksheet 생성
            for (int workSheetNum = 0; workSheetNum < ClassData.Keys.Count; workSheetNum++)
            {
                OutputAllWorkbook.Worksheets.Add(After: OutputAllWorkbook.Worksheets[workSheetNum + 1]);
                var currentWorksheet = OutputAllWorkbook.Worksheets.Item[workSheetNum + 1] as Worksheet;

                Debug.WriteLine(CollegeList[workSheetNum]);

                currentWorksheet.Name = CollegeList[workSheetNum];

                Debug.WriteLine("변경 성공!");
            }

            var lastWorksheet = OutputAllWorkbook.Worksheets.Item[OutputAllWorkbook.Worksheets.Count] as Worksheet;
            lastWorksheet.Name = "부적응Data";
        }

        public void GraphFileTask()
        {
            var graphSheet = OutputGraphWorkbook.Worksheets.Item[1] as Worksheet;
            graphSheet.Name = "그래프Data";
        }

        public void SeparateEachDepart()
        {
            var temptest = ClassData["사범대학"];
            temptest.ForEach(item => Debug.WriteLine("testdata : " + item));

            temptest = ClassData["유스티노자유대학"];
            temptest.ForEach(item => Debug.WriteLine("testdata : " + item));

            // Excel에 값 삽입하는 기본 문법
            // Range rg1 = (Range)OutputAllWorksheet.Cells[1, 1];
            // rg1.Value = "hello world";

            var StudentDepartName = "";
            var tempCollegeName = "";

            var dataRowNum = WholeInputDataRange.Rows.Count;
            var dataColumnNum = WholeInputDataRange.Columns.Count;

            for(int workSheetCount = 1; workSheetCount < OutputAllWorkbook.Worksheets.Count; workSheetCount++)
            {
                Debug.WriteLine(OutputAllWorkbook.Worksheets.Count - workSheetCount + "만큼 반복 수행 시작!!!");

                var targetWorksheet = OutputAllWorkbook.Worksheets.Item[workSheetCount] as Worksheet;
                tempCollegeName = targetWorksheet.Name;

                Debug.WriteLine(tempCollegeName + "작업 준비");

                var tempDepartNameList = ClassData["유스티노자유대학"];

                foreach(string tempDepartName in tempDepartNameList)
                {
                    Debug.WriteLine(tempDepartName + "작업 시작 !!!");
                    Debug.WriteLine(dataRowNum + " 만큼 반복 시작 대기중!!!");
                    for(int rowCount = 1; rowCount < dataRowNum; rowCount++)
                    {
                        if(tempDepartName == (string)(WholeInputDataRange.Cells[rowCount, 1] as Range).Value2)
                        {
                            Debug.WriteLine(rowCount + "번 째 진행중");
                            targetWorksheet.Rows[targetWorksheet.Rows.Count.ToString()] = InputDataWorksheet.Rows[rowCount.ToString()];
                        }
                    }
                }
            }

        }

    }
}