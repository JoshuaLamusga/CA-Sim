using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CASimulator
{
    /// <summary>
    /// Interaction logic for CASimLauncher.xaml
    /// </summary>
    public partial class CASimLauncher : Window
    {
        private enum SimType
        {
            oned,
            twod,
            ant
        }

        private int _rows, _columns, _cellsize, _animspeed;
        private int _type;
        private bool _isToroidal;
        private string _rules;

        public CASimLauncher()
        {
            _rows = 10;
            _columns = 10;
            _cellsize = 4;
            _animspeed = 100;
            _isToroidal = true;
            _type = 1;
            _rules = "See the help for details.";

            InitializeComponent();
        }

        /// <summary>
        /// The simulation should be 1D.
        /// Rules are re-evaluated for 1D.
        /// </summary>
        private void bttn1d_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) //Prevents bttnLaunch from being null.
                return;

            _type = 0;
            chkbxWrap.IsEnabled = true;

            if (CASim1D.CheckRules(txbxRules.Text))
                bttnLaunch.IsEnabled = true;
            else
                bttnLaunch.IsEnabled = false;
        }

        /// <summary>
        /// The simulation should be 2D.
        /// Rules are re-evaluated for 2D.
        /// </summary>
        private void bttn2d_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) //Prevents bttnLaunch from being null.
                return;

            _type = 1;
            chkbxWrap.IsEnabled = true;

            if (CASim2D.CheckRules(txbxRules.Text))
                bttnLaunch.IsEnabled = true;
            else
                bttnLaunch.IsEnabled = false;
        }

        /// <summary>
        /// The simulation should be 2D.
        /// Rules are re-evaluated for 2D.
        /// </summary>
        private void bttnant_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) //Prevents bttnLaunch from being null.
                return;

            _type = 2;
            chkbxWrap.IsEnabled = false;

            if (CASim2DAnt.CheckRules(txbxRules.Text))
                bttnLaunch.IsEnabled = true;
            else
                bttnLaunch.IsEnabled = false;
        }

        /// <summary>
        /// The cell size is changed; non-numeric data is removed.
        /// </summary>
        private void txbxCellSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) //Prevents null exceptions during init.
                return;

            //Text can only be numbers.
            txbxCellSize.Text = ApplyFilter(txbxCellSize.Text, @"\D");

            //Limits text length to 2 digits.
            if (txbxCellSize.Text.Length == 0)
                return;
            else if (txbxCellSize.Text.Length > 2)
                txbxCellSize.Text = txbxCellSize.Text.Substring(0, 2);

            //Limits data range to 1 and 50 inclusive.
            int value = Int32.Parse(txbxCellSize.Text);
            if (value < 1)
                txbxCellSize.Text = "1";
            else if (value > 50)
                txbxCellSize.Text = "50";

            //Sets cell size and reparses text if it changed.
            _cellsize = Int32.Parse(txbxCellSize.Text);
        }

        /// <summary>
        /// The cell rows are changed; non-numeric data is removed.
        /// </summary>
        private void txbxCellRows_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) //Prevents null exceptions during init.
                return;

            //Text can only be numbers.
            txbxCellRows.Text = ApplyFilter(txbxCellRows.Text, @"\D");

            //Limits text length to 4 digits.
            if (txbxCellRows.Text.Length == 0)
                return;
            else if (txbxCellRows.Text.Length > 4)
                txbxCellRows.Text = txbxCellRows.Text.Substring(0, 4);

            //Limits data range to 2 and 2000 inclusive.
            int value = Int32.Parse(txbxCellRows.Text);
            if (value < 2)
                txbxCellRows.Text = "2";
            else if (value > 2000)
                txbxCellRows.Text = "2000";

            //Sets cell rows and reparses text if it changed.
            _rows = Int32.Parse(txbxCellRows.Text);
        }

        /// <summary>
        /// The cell cols are changed; non-numeric data is removed.
        /// This is disabled as long as 1D is enabled.
        /// </summary>
        private void txbxCellCols_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) //Prevents null exceptions during init.
                return;

            //Text can only be numbers.
            txbxCellCols.Text = ApplyFilter(txbxCellCols.Text, @"\D");

            //Limits text length to 4 digits.
            if (txbxCellCols.Text.Length == 0)
                return;
            else if (txbxCellCols.Text.Length > 4)
                txbxCellCols.Text = txbxCellCols.Text.Substring(0, 4);

            //Limits data range to 2 and 2000 inclusive.
            int value = Int32.Parse(txbxCellCols.Text);
            if (value < 2)
                txbxCellCols.Text = "2";
            else if (value > 2000)
                txbxCellCols.Text = "2000";

            //Sets cell columns and reparses text if it changed.
            _columns = Int32.Parse(txbxCellCols.Text);
        }

        /// <summary>
        /// Toggles whether the grid wraps around or not.
        /// This is disabled as long as Langston's ants are enabled.
        /// </summary>
        private void chkbxWrap_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) //prevents null exceptions during init.
                return;

            //Returns false if IsChecked is null.
            _isToroidal = (chkbxWrap.IsChecked ?? false);
        }

        /// <summary>
        /// The update speed is changed; non-numeric data is removed.
        /// Update speeds are restricted to values > 4 and less than 10000.
        /// Update speeds are rounded to integers.
        /// </summary>
        private void txbxUpdateSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) //Prevents null exceptions during init.
                return;

            //Text can only be numbers.
            txbxUpdateSpeed.Text = ApplyFilter(txbxUpdateSpeed.Text, @"\D");

            //Limits text length to 5 digits.
            if (txbxUpdateSpeed.Text.Length == 0)
                return;
            else if (txbxUpdateSpeed.Text.Length > 5)
                txbxUpdateSpeed.Text = txbxUpdateSpeed.Text.Substring(0, 5);

            //Limits data range to 5 and 10000 inclusive.
            int value = Int32.Parse(txbxUpdateSpeed.Text);
            if (value < 5)
                txbxUpdateSpeed.Text = "5";
            else if (value > 10000)
                txbxUpdateSpeed.Text = "10000";

            //Sets cell columns and reparses text if it changed.
            _animspeed = Int32.Parse(txbxUpdateSpeed.Text);
        }

        /// <summary>
        /// The rules are changed.
        /// The launch button is disabled if rules are invalid.
        /// </summary>
        private void txbxRules_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) //Prevents null exceptions during init.
                return;

            switch (_type)
            {
                case 0:
                    if (CASim1D.CheckRules(txbxRules.Text))
                        bttnLaunch.IsEnabled = true;
                    else
                        bttnLaunch.IsEnabled = false;
                    break;
                case 1:
                    if (CASim2D.CheckRules(txbxRules.Text))
                        bttnLaunch.IsEnabled = true;
                    else
                        bttnLaunch.IsEnabled = false;
                    break;
                case 2:
                    if (CASim2DAnt.CheckRules(txbxRules.Text))
                        bttnLaunch.IsEnabled = true;
                    else
                        bttnLaunch.IsEnabled = false;
                    break;
            }
            _rules = txbxRules.Text;
        }

        /// <summary>
        /// Launches an automaton simulation window with the given settings.
        /// The launch button is disabled if rules are invalid.
        /// </summary>
        private void bttnLaunch_Click(object sender, RoutedEventArgs e)
        {
            switch (_type)
            {
                case 0:
                    CASim1DGui simDialog2 = new CASim1DGui(_columns,
                    _cellsize,
                    _isToroidal,
                    _rules,
                    new TimeSpan(0, 0, 0, 0, _animspeed));

                    simDialog2.cellColors = CAPresets.ColorsBinary;
                    simDialog2.Show();
                    simDialog2.UpdateGui();
                    break;
                case 1:
                    CASim2DGui simDialog1 = new CASim2DGui(_rows,
                    _columns,
                    _cellsize,
                    _isToroidal,
                    _rules,
                    new TimeSpan(0, 0, 0, 0, _animspeed));

                    simDialog1.cellColors = CAPresets.ColorsBinary;
                    simDialog1.Show();
                    simDialog1.UpdateGui(true);
                    break;
                case 2:
                    CASim2DAntGui simDialog3 = new CASim2DAntGui(new CAAnt2D[0],
                    _rows,
                    _columns,
                    _cellsize,
                    _rules,
                    new TimeSpan(0, 0, 0, 0, _animspeed));

                    simDialog3.cellColors = CAPresets.ColorsRainbow;
                    simDialog3.antColors = CAPresets.ColorsGrayscale8;
                    simDialog3.Show();
                    simDialog3.UpdateGui(true);
                    break;
            }
        }

        /// <summary>
        /// Removes all matches of the given regex.
        /// This is used for filtering textboxes on TextChanged events.
        /// </summary>
        private string ApplyFilter(string text, string filter)
        {
            if (filter != "" && filter != null &&
                text != "" && text != null)
            {
                MatchCollection matches = Regex.Matches(text, filter);
                foreach (Match match in matches)
                {
                    text = text.Replace(match.Value, "");
                }
            }

            return text;
        }

        /// <summary>
        /// Displays a simple dialog with helpful information about formatting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bttnHelp_Click(object sender, RoutedEventArgs e)
        {
            CASimAbout aboutDlg = new CASimAbout();
            aboutDlg.Show();
        }
    }
}