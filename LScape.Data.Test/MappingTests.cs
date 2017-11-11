using LScape.Data.Mapping;
using LScape.Data.Test.TestObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace LScape.Data.Test
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void SimpleMapping()
        {
            var map = new Map<TestUser>();
            Assert.IsTrue(map.TableName == "TestUser");
            Assert.AreEqual(10, map.Fields.Count());

            foreach (var field in map.Fields)
            {
                Assert.AreEqual(field.PropertyName, field.ColumnName, $"On field {field.PropertyName}");

                if(field.PropertyName == "Groups")
                    Assert.AreEqual(FieldType.Ignore, field.FieldType, $"On field {field.PropertyName}");
                else
                    Assert.AreEqual(FieldType.Map, field.FieldType, $"On field {field.PropertyName}");
            }
        }

        [TestMethod]
        public void CheckConventions()
        {
            var config = new MapperConfiguration {
                ColumnNameConvention = NameConvention.SplitCaseLower,
                CalculatedMatch = (s, t) => s == "Deleted" && t == typeof(DateTime?),
                KeyMatch = (s, t) => s == "Id" && t == typeof(Guid)
            };

            var map = new Map<TestUser>(config);
            Assert.IsTrue(map.TableName == "TestUser");
            Assert.AreEqual(10, map.Fields.Count());

            foreach (var field in map.Fields)
            {
                if(field.PropertyName == "FirstName")
                    Assert.AreEqual("first_name", field.ColumnName);
                else if(field.PropertyName == "LastName")
                    Assert.AreEqual("last_name", field.ColumnName);
                else if(field.PropertyName == "TestEnum")
                    Assert.AreEqual("test_enum", field.ColumnName);
                else
                    Assert.AreEqual(field.PropertyName.ToLower(), field.ColumnName, $"On field {field.PropertyName}");

                if(field.PropertyName == "Id")
                    Assert.AreEqual(FieldType.Key, field.FieldType);
                else if(field.PropertyName == "Deleted")
                    Assert.AreEqual(FieldType.Calculated, field.FieldType);
                else if (field.PropertyName == "Groups")
                    Assert.AreEqual(FieldType.Ignore, field.FieldType);
                else
                    Assert.AreEqual(FieldType.Map, field.FieldType, $"On field {field.PropertyName}");
            }
        }

        [TestMethod]
        public void TestStrings()
        {
            var config = new MapperConfiguration {
                ColumnNameConvention = NameConvention.SplitCaseLower,
                CalculatedMatch = (s, t) => s == "Deleted" && t == typeof(DateTime?),
                KeyMatch = (s, t) => s == "Id" && t == typeof(Guid)
            };
            var map = new Map<TestUser>(config);

            Assert.AreEqual("[id], [email], [first_name], [last_name], [password], [salt], [created], [deleted], [test_enum]", map.SelectColumnList);
            Assert.AreEqual("a.[id], a.[email], a.[first_name], a.[last_name], a.[password], a.[salt], a.[created], a.[deleted], a.[test_enum]", map.SelectColumnWithAlias("a"));
            Assert.AreEqual("[email], [first_name], [last_name], [password], [salt], [created], [test_enum]", map.InsertColumnList);
            Assert.AreEqual("@email, @first_name, @last_name, @password, @salt, @created, @test_enum", map.InsertParameterList);
            Assert.AreEqual("[email] = @email, [first_name] = @first_name, [last_name] = @last_name, [password] = @password, [salt] = @salt, [created] = @created, [test_enum] = @test_enum", map.UpdateSetString);
        }

        [TestMethod]
        public void TestParameterList()
        {
            var config = new MapperConfiguration {
                ColumnNameConvention = NameConvention.SplitCaseLower,
                CalculatedMatch = (s, t) => s == "Deleted" && t == typeof(DateTime?),
                KeyMatch = (s, t) => s == "Id" && t == typeof(Guid)
            };
            var map = new Map<TestUser>(config);

            var idGuid = Guid.Parse("36a5a92c-05a6-4375-bb1f-38d1a56c93dd");
            var created = DateTime.UtcNow.AddHours(-1);
            var testUser = new TestUser {
                Id = idGuid,
                Email = "Test@test.com",
                FirstName = "TestFirst",
                LastName = "TestLast",
                Password = new byte[] {0, 4, 5, 6},
                Salt = new byte[] {0, 1, 2, 3},
                Created = created,
                TestEnum = TestEnum.Test2
            };

            var command = new SqlCommand();
            map.AddParameters(command, testUser, true);

            Assert.AreEqual(8, command.Parameters.Count);
            foreach (SqlParameter parameter in command.Parameters)
            {
                if (parameter.ParameterName == "id")
                {
                    Assert.AreEqual(DbType.Guid, parameter.DbType);
                    Assert.AreEqual(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
                    Assert.AreEqual(idGuid, parameter.Value);
                }
                else if (parameter.ParameterName == "email")
                {
                    Assert.AreEqual(DbType.String, parameter.DbType);
                    Assert.AreEqual(SqlDbType.NVarChar, parameter.SqlDbType);
                    Assert.AreEqual("Test@test.com", parameter.Value);
                }
                else if (parameter.ParameterName == "first_name")
                {
                    Assert.AreEqual(DbType.String, parameter.DbType);
                    Assert.AreEqual(SqlDbType.NVarChar, parameter.SqlDbType);
                    Assert.AreEqual("TestFirst", parameter.Value);
                }
                else if (parameter.ParameterName == "last_name")
                {
                    Assert.AreEqual(DbType.String, parameter.DbType);
                    Assert.AreEqual(SqlDbType.NVarChar, parameter.SqlDbType);
                    Assert.AreEqual("TestLast", parameter.Value);
                }
                else if (parameter.ParameterName == "password")
                {
                    Assert.AreEqual(DbType.Binary, parameter.DbType);
                    Assert.AreEqual(SqlDbType.VarBinary, parameter.SqlDbType);
                    Assert.IsTrue(StructuralComparisons.StructuralComparer.Compare(new byte[] { 0, 4, 5, 6 }, parameter.Value) == 0);
                }
                else if (parameter.ParameterName == "salt")
                {
                    Assert.AreEqual(DbType.Binary, parameter.DbType);
                    Assert.AreEqual(SqlDbType.VarBinary, parameter.SqlDbType);
                    Assert.IsTrue(StructuralComparisons.StructuralComparer.Compare(new byte[] { 0, 1, 2, 3 }, parameter.Value) == 0);
                }
                else if (parameter.ParameterName == "created")
                {
                    Assert.AreEqual(DbType.DateTime2, parameter.DbType);
                    Assert.AreEqual(SqlDbType.DateTime2, parameter.SqlDbType);
                    Assert.AreEqual(created, parameter.Value);
                }
                else if (parameter.ParameterName == "test_enum")
                {
                    Assert.AreEqual(DbType.Byte, parameter.DbType);
                    Assert.AreEqual(SqlDbType.TinyInt, parameter.SqlDbType);
                    Assert.AreEqual(TestEnum.Test2, parameter.Value);
                }
                else
                    Assert.Fail($"Parameter {parameter.ParameterName} should not be in the list");
            }
        }
    }
}
