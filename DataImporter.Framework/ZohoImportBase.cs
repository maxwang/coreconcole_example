using DataImporter.Framework.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DataImporter.Framework
{
    public abstract class ZohoImportBase
    {
        protected readonly IZohoCRMDataRepository _zohoRepository;

        public ZohoImportBase(IZohoCRMDataRepository zohoRepository)
        {
            _zohoRepository = zohoRepository;
        }

        public void test()
        {
            Console.WriteLine("test");
        }
        
    }
}
