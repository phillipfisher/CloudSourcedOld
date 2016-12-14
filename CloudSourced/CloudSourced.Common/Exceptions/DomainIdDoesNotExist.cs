using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Exceptions
{
    public class DomainIdDoesNotExist : CloudSourcedException
    {
        public readonly string Container;
        public readonly string Domain;
        public readonly string Id;

        public DomainIdDoesNotExist(string container, string domain, string id) : base ($"Domain Id does not exists: /{container}/{domain}/{id}.events")
        {
            Container = container;
            Domain = domain;
            Id = id;
        }
    }
}
