using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Console.Tests
{
    public class DemoTests
    {
        [Fact]
        public void SingleFilter_Works()
        {
            var context = new XrmFakedContext();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "test"
            };

            var employment = new Employment
            {
                Id = Guid.NewGuid(),
                PersonId = person.ToEntityReference(),
                StartDate = null,
                EndDate = null
            };

            context.Initialize(new List<Entity>
            {
                person,
                employment
            });

            var service = context.GetOrganizationService();

            var query = new QueryExpression(Person.EntityLogicalName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(Person.AttributeLogicalNames.name),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(Person.AttributeLogicalNames.name,ConditionOperator.Like,"test")
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Person.EntityLogicalName,
                        LinkFromAttributeName = Person.AttributeLogicalNames.Id,
                        LinkToEntityName = Employment.EntityLogicalName,
                        LinkToAttributeName = Employment.AttributeLogicalNames.PersonId,
                        JoinOperator = JoinOperator.Inner,
                        Columns = new ColumnSet(false),
                        EntityAlias = Employment.EntityLogicalName,
                        LinkCriteria =
                        {
                            Filters =
                            {
                                new FilterExpression(LogicalOperator.Or)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression(Employment.AttributeLogicalNames.StartDate,ConditionOperator.Null),
                                        new ConditionExpression(Employment.AttributeLogicalNames.EndDate,ConditionOperator.Null)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var results = service.RetrieveMultiple(query).Entities.Cast<Person>().ToList();
            Assert.Single(results);
        }

        [Fact]
        public void NestedFilter_Fails()
        {
            var context = new XrmFakedContext();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "test"
            };

            var employment = new Employment
            {
                Id = Guid.NewGuid(),
                PersonId = person.ToEntityReference(),
                StartDate = null,
                EndDate = null
            };

            context.Initialize(new List<Entity>
            {
                person,
                employment
            });

            var service = context.GetOrganizationService();

            var query = new QueryExpression(Person.EntityLogicalName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(Person.AttributeLogicalNames.name),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(Person.AttributeLogicalNames.name,ConditionOperator.Like,"test")
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Person.EntityLogicalName,
                        LinkFromAttributeName = Person.AttributeLogicalNames.Id,
                        LinkToEntityName = Employment.EntityLogicalName,
                        LinkToAttributeName = Employment.AttributeLogicalNames.PersonId,
                        JoinOperator = JoinOperator.Inner,
                        Columns = new ColumnSet(false),
                        EntityAlias = Employment.EntityLogicalName,
                        LinkCriteria =
                        {
                            Filters =
                            {
                                //The following code works with a nested filter to set up a query with a condition in the form of: ((A && B) || (C && D))
                                //As soon as you comment out the nested conditions everything works fine
                                //If you run the code with them enabled you get an error: XrmFakedContext.FindReflectedAttributeType: Attribute startdate not found for type Console.Person
                                //It looks like because of the nesting, somehow the framework looks for the attribute on the wrong type? (On person, while the actual attribute is on Employment)
                                new FilterExpression(LogicalOperator.Or)
                                {
                                    Filters =
                                    {
                                        new FilterExpression(LogicalOperator.And)
                                        {
                                            Conditions =
                                            {
                                                new ConditionExpression(Employment.AttributeLogicalNames.StartDate,ConditionOperator.NotNull),
                                                new ConditionExpression(Employment.AttributeLogicalNames.EndDate,ConditionOperator.Null)
                                            }
                                        },
                                        new FilterExpression(LogicalOperator.And)
                                        {
                                            Conditions =
                                            {
                                                new ConditionExpression(Employment.AttributeLogicalNames.StartDate,ConditionOperator.Null),
                                                new ConditionExpression(Employment.AttributeLogicalNames.EndDate,ConditionOperator.NotNull)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var results = service.RetrieveMultiple(query).Entities.Cast<Person>().ToList();
            Assert.Single(results);
        }
    }
}