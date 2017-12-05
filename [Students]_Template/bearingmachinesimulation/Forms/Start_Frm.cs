using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using BearingMachineModels;

namespace BearingMachineSimulation.Forms
{
    public partial class Form1 : Form
    {
        private readonly SimulationSystem _simulationSystem;
        private readonly List<TimeDistribution> _bearingDistributions;
        private readonly List<TimeDistribution> _delayDistributions;
        private readonly List<CurrentSimulationCase> _currentSimulationCases;
        private readonly List<ProposedSimulationCase> _proposedSimulationCases;
        private readonly Random _rnd;
        public Form1()
        {
            InitializeComponent();
            _simulationSystem = new SimulationSystem();
            _bearingDistributions = new List<TimeDistribution>();
            _delayDistributions = new List<TimeDistribution>();
            _currentSimulationCases = new List<CurrentSimulationCase>();
            _proposedSimulationCases = new List<ProposedSimulationCase>();
            _rnd = new Random();
        }

        public void Intialize()
        {
            {
                _simulationSystem.DowntimeCost = Convert.ToInt32(DownTime_txt.Text);
                _simulationSystem.RepairPersonCost = Convert.ToInt32(RepairPersonCost_txt.Text);
                _simulationSystem.BearingCost = Convert.ToInt32(NumBears_txt.Text);
                _simulationSystem.NumberOfHours = Convert.ToInt32(NumHours_txt.Text);
                _simulationSystem.NumberOfBearings = Convert.ToInt32(NumBears_txt.Text);
                _simulationSystem.RepairTimeForOneBearing = Convert.ToInt32(RepairTime_txt.Text);
                _simulationSystem.RepairTimeForAllBearings = Convert.ToInt32(RepairTimeAll_txt.Text);
            }

            decimal cumProb = 0;
            foreach (DataGridViewRow row in BearingLife_DGV.Rows)
            {
                if (row.Cells[0].Value == null)
                    break;

                TimeDistribution tmpTimeDistribution =
                    new TimeDistribution
                    {
                        Time = Convert.ToInt32(row.Cells[0].Value.ToString()),
                        Probability = Convert.ToDecimal(row.Cells[1].Value.ToString())
                    };
                if (cumProb == 0)
                    tmpTimeDistribution.MinRange = 1;
                else
                    tmpTimeDistribution.MinRange = Convert.ToInt32(cumProb * 100) + 1;

                tmpTimeDistribution.CummProbability = cumProb + tmpTimeDistribution.Probability;
                cumProb += tmpTimeDistribution.Probability;
                tmpTimeDistribution.MaxRange = Convert.ToInt32(cumProb * 100);

                _bearingDistributions.Add(tmpTimeDistribution);
            }

            cumProb = 0;
            foreach (DataGridViewRow row in DelayTime_DGV.Rows)
            {
                if (row.Cells[0].Value == null)
                    break;

                TimeDistribution tmpTimeDistribution =
                    new TimeDistribution
                    {
                        Time = Convert.ToInt32(row.Cells[0].Value.ToString()),
                        Probability = Convert.ToDecimal(row.Cells[1].Value.ToString())
                    };
                if (cumProb == 0)
                    tmpTimeDistribution.MinRange = 1;
                else
                    tmpTimeDistribution.MinRange = Convert.ToInt32(cumProb * 10) + 1;

                tmpTimeDistribution.CummProbability = cumProb + tmpTimeDistribution.Probability;
                cumProb += tmpTimeDistribution.Probability;
                tmpTimeDistribution.MaxRange = Convert.ToInt32(cumProb * 10);

                _delayDistributions.Add(tmpTimeDistribution);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Intialize();
            Construst_concurrentPolicy();
            Construct_proposedPolicy();
            for (int i = 0; i < BearingLife_DGV.Rows.Count - 1; i++)
            {
                BearingLife_DGV.Rows[i].Cells[2].Value = _bearingDistributions[i].CummProbability;
                BearingLife_DGV.Rows[i].Cells[3].Value = _bearingDistributions[i].MinRange + " - " +
                                                         _bearingDistributions[i].MaxRange;

            }
            for (int i = 0; i < DelayTime_DGV.Rows.Count - 1; i++)
            {
                DelayTime_DGV.Rows[i].Cells[2].Value = _delayDistributions[i].CummProbability;
                DelayTime_DGV.Rows[i].Cells[3].Value = _delayDistributions[i].MinRange + " - " +
                                                       _delayDistributions[i].MaxRange;

            }
            for (int i = 0; i < _simulationSystem.CurrentSimulationCases.Count; i++)
            {
                Current_DGV.Rows.Add((i + 1).ToString(),
                    _simulationSystem.CurrentSimulationCases[i].Bearing.Index.ToString(),
                    _simulationSystem.CurrentSimulationCases[i].Bearing.RandomHours.ToString(),
                    _simulationSystem.CurrentSimulationCases[i].Bearing.Hours.ToString(),
                    _simulationSystem.CurrentSimulationCases[i].AccumulatedHours.ToString(),
                    _simulationSystem.CurrentSimulationCases[i].RandomDelay.ToString(),
                    _simulationSystem.CurrentSimulationCases[i].Delay.ToString());
            }
            
            Proposed_DGV.Columns.Add("column1", "#");
            for (int i = 0; i < _simulationSystem.NumberOfBearings; i++)
            {
                Proposed_DGV.Columns.Add("column" + (i + 2), "Bearing " + (i + 1));
            }
            Proposed_DGV.Columns.Add("column" + _simulationSystem.NumberOfBearings + 2, "First Failure");
            Proposed_DGV.Columns.Add("column" + _simulationSystem.NumberOfBearings + 3, "Accumulated Hours");
            Proposed_DGV.Columns.Add("column" + _simulationSystem.NumberOfBearings + 4, "RD");
            Proposed_DGV.Columns.Add("column" + _simulationSystem.NumberOfBearings + 5, "Delay (Minutes)");

            for (int i = 0; i < _simulationSystem.ProposedSimulationCases.Count; i++)
            {
                
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewCell cell = new DataGridViewButtonCell();
                cell.Value = i + 1;
                row.Cells.Add(cell);
                
                for (int j = 0; j < _simulationSystem.NumberOfBearings; j++)
                {
                    cell = new DataGridViewButtonCell();
                    cell.Value = _simulationSystem.ProposedSimulationCases[i].Bearings[j].Hours;
                    row.Cells.Add(cell);

                }

                cell = new DataGridViewButtonCell();
                cell.Value = _simulationSystem.ProposedSimulationCases[i].FirstFailure;
                row.Cells.Add(cell);
                cell = new DataGridViewButtonCell();
                cell.Value = _simulationSystem.ProposedSimulationCases[i].AccumulatedHours;
                row.Cells.Add(cell);
                cell = new DataGridViewButtonCell();
                cell.Value = _simulationSystem.ProposedSimulationCases[i].RandomDelay;
                row.Cells.Add(cell);
                cell = new DataGridViewButtonCell();
                cell.Value = _simulationSystem.ProposedSimulationCases[i].Delay;
                row.Cells.Add(cell);
                
                Proposed_DGV.Rows.Add(row);
            }
        }

        private void Construst_concurrentPolicy()
        {
            List<CurrentSimulationCase> currentSimulationCases = new List<CurrentSimulationCase>();
            PerformanceMeasures currentPerformanceMeasures = new PerformanceMeasures();

            for (int i = 1; i <= _simulationSystem.NumberOfBearings; i++)
            {
                int accHours = 0;
                while (true)
                {
                    CurrentSimulationCase currentSimulation = new CurrentSimulationCase();
                    Bearing bearing = new Bearing();
                    bearing.Index = i;
                    bearing.RandomHours = _rnd.Next(1, 101);
                    bearing.Hours = Get_Hours(bearing.RandomHours);

                    currentSimulation.Bearing = bearing;
                    accHours += bearing.Hours;
                    currentSimulation.AccumulatedHours = accHours;
                    currentSimulation.RandomDelay = _rnd.Next(1, 11);
                    currentSimulation.Delay = Get_Delay(currentSimulation.RandomDelay);
                    currentSimulationCases.Add(currentSimulation);
                    if (currentSimulation.AccumulatedHours >= _simulationSystem.NumberOfHours)
                        break;
                }
            }
            _simulationSystem.CurrentSimulationCases = currentSimulationCases;

            currentPerformanceMeasures.BearingCost =
                _simulationSystem.CurrentSimulationCases.Count * _simulationSystem.BearingCost;
            currentPerformanceMeasures.DelayCost = Get_Delay_Accmulated() * _simulationSystem.DowntimeCost;
            currentPerformanceMeasures.DowntimeCost =
                _simulationSystem.CurrentSimulationCases.Count * _simulationSystem.RepairTimeForOneBearing *
                _simulationSystem.DowntimeCost;
            decimal val = Convert.ToDecimal(_simulationSystem.RepairPersonCost) / 60;
            currentPerformanceMeasures.RepairPersonCost = _simulationSystem.CurrentSimulationCases.Count *
                                                          _simulationSystem.RepairTimeForOneBearing * val;
                                                         
            currentPerformanceMeasures.TotalCost = currentPerformanceMeasures.BearingCost +
                                                   currentPerformanceMeasures.DelayCost +
                                                   currentPerformanceMeasures.DowntimeCost +
                                                   currentPerformanceMeasures.RepairPersonCost;
            _simulationSystem.CurrentPerformanceMeasures = currentPerformanceMeasures;
            TotlBearingCost_lbl.Text = currentPerformanceMeasures.BearingCost.ToString(CultureInfo.CurrentCulture);
            TotlCost_lbl.Text = currentPerformanceMeasures.TotalCost.ToString(CultureInfo.CurrentCulture);
            TotlDelayCost_lbl.Text = currentPerformanceMeasures.DelayCost.ToString(CultureInfo.CurrentCulture);
            TotlDownCost_lbl.Text = currentPerformanceMeasures.DowntimeCost.ToString(CultureInfo.CurrentCulture);
            TotlRepairPerson_lbl.Text = currentPerformanceMeasures.RepairPersonCost.ToString(CultureInfo.CurrentCulture);

        }

        private void Construct_proposedPolicy()
        {
            int accHours = 0;
            int order = 1;
            while (true)
            {
                ProposedSimulationCase tmpSimulationCase = new ProposedSimulationCase();
                List<Bearing> tmpbearings = new List<Bearing>();
                List<int> Hours = new List<int>();
                for (int i = 0; i < _simulationSystem.NumberOfBearings; i++)
                {
                    Bearing tmpBearing = new Bearing();
                    tmpBearing.Hours = Get_HoursOfind(order, i + 1);
                    tmpbearings.Add(tmpBearing);
                    Hours.Add(tmpBearing.Hours);
                }
                order++;
                Hours.Sort();
                tmpSimulationCase.Bearings = tmpbearings;
                tmpSimulationCase.FirstFailure = Hours[0];
                accHours += tmpSimulationCase.FirstFailure;
                tmpSimulationCase.AccumulatedHours = accHours;
                tmpSimulationCase.RandomDelay = _rnd.Next(1, 11);
                tmpSimulationCase.Delay = Get_Delay(tmpSimulationCase.RandomDelay);
                _proposedSimulationCases.Add(tmpSimulationCase);
                if (accHours >= _simulationSystem.NumberOfHours)
                    break;
            }
            _simulationSystem.ProposedSimulationCases = _proposedSimulationCases;
            PerformanceMeasures tmpMeasures = new PerformanceMeasures();
            tmpMeasures.BearingCost =
                (_simulationSystem.NumberOfBearings * _simulationSystem.ProposedSimulationCases.Count) *
                _simulationSystem.BearingCost;
            tmpMeasures.DelayCost = Get_Sum_proposed_Delay(_simulationSystem.ProposedSimulationCases) *
                                    _simulationSystem.DowntimeCost;
            tmpMeasures.DowntimeCost = _simulationSystem.ProposedSimulationCases.Count *
                                       _simulationSystem.RepairTimeForAllBearings * _simulationSystem.DowntimeCost;
            Decimal val = Convert.ToDecimal(_simulationSystem.RepairPersonCost) / 60;
            tmpMeasures.RepairPersonCost = _simulationSystem.ProposedSimulationCases.Count *
                                           _simulationSystem.RepairTimeForAllBearings * val;
            tmpMeasures.TotalCost = tmpMeasures.BearingCost + tmpMeasures.DelayCost + tmpMeasures.DowntimeCost +
                                    tmpMeasures.RepairPersonCost;
            _simulationSystem.ProposedPerformanceMeasures = tmpMeasures;

            TotlBearingCost_lbl1.Text = tmpMeasures.BearingCost.ToString(CultureInfo.CurrentCulture);
            TotlDelayCost_lbl1.Text = tmpMeasures.DelayCost.ToString(CultureInfo.CurrentCulture);
            TotlDownCost_lbl1.Text = tmpMeasures.DowntimeCost.ToString(CultureInfo.CurrentCulture);
            TotlRepairPerson_lbl1.Text = tmpMeasures.RepairPersonCost.ToString(CultureInfo.CurrentCulture);
            TotlCost_lbl1.Text = tmpMeasures.TotalCost.ToString(CultureInfo.CurrentCulture);


        }
        private int Get_Delay_Accmulated()
        {
            int sum = 0;
            foreach (CurrentSimulationCase simulationCase in _simulationSystem.CurrentSimulationCases)
            {
                sum += simulationCase.Delay;
            }
            return sum;
        }

        private int Get_Hours(int randomHours)
        {
            foreach (TimeDistribution time in _bearingDistributions)
            {
                if (randomHours >= time.MinRange &&
                    randomHours <= time.MaxRange)
                {
                    return time.Time;
                }
            }
            return 0;
        }

        private int Get_Delay(int randomDelay)
        {
            foreach (TimeDistribution time in _delayDistributions)
            {
                if (randomDelay >= time.MinRange &&
                    randomDelay <= time.MaxRange)
                {
                    return time.Time;
                }
            }
            return 0;
        }

        private int Get_HoursOfind(int order, int NumOfBearing)
        {
            List<Bearing> myBearings = new List<Bearing>();

            for (int i = 0; i < _simulationSystem.CurrentSimulationCases.Count; i++)
            {
                if (NumOfBearing == _simulationSystem.CurrentSimulationCases[i].Bearing.Index)
                {
                    myBearings.Add(_simulationSystem.CurrentSimulationCases[i].Bearing);
                }
            }

            if (order <= myBearings.Count)
            {
                return myBearings[order - 1].Hours;
            }


            int randomHours = _rnd.Next(1, 101);
            return Get_Hours(randomHours);

        }

        private int Get_Sum_proposed_Delay(List<ProposedSimulationCase> tmpCases)
        {
            int sum = 0;
            foreach (ProposedSimulationCase _case in tmpCases)
            {
                sum += _case.Delay;
            }
            return sum;
        }

        private void Process()
        {
            var streamReader = new StreamReader(@"C:\Users\Ibrahim Hasan\Desktop\[Students]_Template\bearingmachinesimulation\TestCases\TestCase1.txt");
            string tempLine;
            List<string> linesList = new List<string>();

            while ((tempLine = streamReader.ReadLine()) != null)
            {
                linesList.Add(tempLine);
            }

            ///////////////
            for (var i = 0; i < linesList.Count; i++)
            {
                if (linesList[i].Contains("DowntimeCost"))
                {
                    DownTime_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("RepairPersonCost"))
                {
                    RepairPersonCost_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("BearingCost"))
                {
                    BearsCost_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("NumberOfHours"))
                {
                    NumHours_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("NumberOfBearings")) 
                {
                    NumBears_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("RepairTimeForOneBearing"))
                {
                    RepairTime_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("RepairTimeForAllBearings"))
                {
                    RepairTimeAll_txt.Text = linesList[++i];
                }
                else if (linesList[i].Contains("DelayTimeDistribution"))
                {
                    i++;
                    while (linesList[i].Contains(","))
                    {
                        string[] split = linesList[i].Replace(" ", "").Split(',');
                        DelayTime_DGV.Rows.Add(split[0], split[1], "", "");
                        i++;
                    }
                    
                    
                }
                else if (linesList[i].Contains("BearingLifeDistribution"))
                {
                    while (i < linesList.Count -1)
                    {
                        string[] split = linesList[++i].Replace(" ", "").Split(',');
                        BearingLife_DGV.Rows.Add(split[0], split[1], "", "");
                    }
                }
            }
            streamReader.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process();
        }
    }
}
