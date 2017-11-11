using System;
using System.Collections.Generic;

namespace LScape.Data.Test.TestObjects
{
    public class TestUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Deleted { get; set; }
        public TestEnum TestEnum { get; set; }

        public List<TestGroup> Groups { get; set; }
    }
}
