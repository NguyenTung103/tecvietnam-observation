﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bts.udpgateway
{
    [Table("ReportDailyMucNuoc")]
    public partial class ReportDailyMucNuoc
    {
        public int Id { get; set; }

        public Nullable<int> DeviceId { get; set; }

        public string ReportTypeCode { get; set; }

        public Nullable<System.DateTime> DateReport { get; set; }

        public string ContentReport { get; set; }

        public Nullable<System.DateTime> DateRequestReport { get; set; }

        public Nullable<System.DateTime> TimeMaxValue { get; set; }

        public Nullable<System.DateTime> TimeMinValue { get; set; }

        public Nullable<double> Distance1 { get; set; }

        public Nullable<double> Distance2 { get; set; }

        public Nullable<double> Distance3 { get; set; }

        public Nullable<double> Distance4 { get; set; }

        public Nullable<double> Distance5 { get; set; }

        public Nullable<double> Distance6 { get; set; }

        public Nullable<double> Distance7 { get; set; }

        public Nullable<double> Distance8 { get; set; }

        public Nullable<double> Distance9 { get; set; }

        public Nullable<double> Distance10 { get; set; }

        public Nullable<double> Distance11 { get; set; }

        public Nullable<double> Distance12 { get; set; }

        public Nullable<double> Distance13 { get; set; }

        public Nullable<double> Distance14 { get; set; }

        public Nullable<double> Distance15 { get; set; }

        public Nullable<double> Distance16 { get; set; }

        public Nullable<double> Distance17 { get; set; }

        public Nullable<double> Distance18 { get; set; }

        public Nullable<double> Distance19 { get; set; }

        public Nullable<double> Distance20 { get; set; }

        public Nullable<double> Distance21 { get; set; }

        public Nullable<double> Distance22 { get; set; }

        public Nullable<double> Distance23 { get; set; }

        public Nullable<double> Distance24 { get; set; }
    }
}
