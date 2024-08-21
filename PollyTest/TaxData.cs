using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyTest
{
    public class TaxDataReadContext
    {
        public BindingContext BindingContext { get; set; }

        public bool UseSnapshotTom { get; set; }
    }

    public class BindingContext
    {
        public string ReturnId { get; set; }
        public string TaxYear { get; set; }
        public string ApplicationId { get; set; }
        public string FormName { get; set; }
        public string TaxSystem { get; set; }
    }
}
