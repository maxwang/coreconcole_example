using System;
using System.Collections.Generic;
using System.Text;
using DataImporter.Framework.Repository;

namespace DataImporter.Framework
{
    public class PartnerPortalImporter : ZohoImportBase
    {
        public PartnerPortalImporter(IZohoCRMDataRepository zohoRepository) : base(zohoRepository)
        {
        }

        public void test()
        {
            var i = 0;
            Console.WriteLine(i);

        }
    }
}
