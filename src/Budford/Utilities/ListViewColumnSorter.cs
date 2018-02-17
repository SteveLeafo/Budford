using System;
using System.Collections;
using System.Windows.Forms;

namespace Budford.Utilities
{

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        internal int ColumnToSort ;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        internal SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        internal int SortType = 0;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 1;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.Ascending;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult = 0;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            if (SortType == 1)
            {
                int a = 0, b = 0;

                if (ColumnToSort < listviewX.SubItems.Count)
                {
                    if (ColumnToSort < listviewX.SubItems.Count)
                    {
                        int.TryParse(listviewX.SubItems[ColumnToSort].Text, out a);
                        int.TryParse(listviewY.SubItems[ColumnToSort].Text, out b);
                    }
                }

                compareResult = b.CompareTo(a);
            }
            else if (SortType == 2)
            {
                int a = 0, b = 0;

                if (ColumnToSort < listviewX.SubItems.Count)
                {
                    if (ColumnToSort < listviewX.SubItems.Count)
                    {
                        int.TryParse(listviewX.SubItems[ColumnToSort].Text.Replace(" MB", "").Replace(",", ""), out a);
                        int.TryParse(listviewY.SubItems[ColumnToSort].Text.Replace(" MB", "").Replace(",", ""), out b);
                    }
                }
                compareResult = b.CompareTo(a);
            }
            else if (SortType == 3)
            {
                DateTime a = DateTime.MinValue, b = DateTime.MinValue;
                if (ColumnToSort < listviewX.SubItems.Count)
                {
                    if (ColumnToSort < listviewX.SubItems.Count)
                    {
                        DateTime.TryParse(listviewX.SubItems[ColumnToSort].Text.Replace(" MB", "").Replace(",", ""), out a);
                        DateTime.TryParse(listviewY.SubItems[ColumnToSort].Text.Replace(" MB", "").Replace(",", ""), out b);
                    }
                }

                compareResult = b.CompareTo(a);
            }
            else
            {
                if (ColumnToSort < listviewX.SubItems.Count)
                {
                    if (ColumnToSort < listviewX.SubItems.Count)
                    {
                        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
                    }
                }
            }

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }
    }
}
