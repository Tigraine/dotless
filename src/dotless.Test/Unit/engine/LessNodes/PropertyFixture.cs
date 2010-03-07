/* Copyright 2009 dotless project, http://www.dotlesscss.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License. */

using System.Collections.Generic;

namespace dotless.Test.Unit.engine
{
    using Core.engine;
    using NUnit.Framework;

    [TestFixture]
    public class PropertyFixture
    {
        [Test]
        public void CanEvaluateColorProperties()
        {
            var list = new List<INode>
                           {
                               new Color(1, 1, 1),
                               new Operator("+"),
                               new Color(1, 1, 1),
                               new Operator("*"),
                               new Number(20)
                           };
            var exp = new Expression(list);
            var prop = new Property("background-color", new []{exp});

            Assert.That(prop.ToCss(), Is.EqualTo("background-color: #151515;"));

            var newColor = prop.Evaluate();

            Assert.That(newColor.ToString(), Is.EqualTo("#151515"));
            Assert.That(newColor, Is.TypeOf<Color>());
        }

        [Test]
        public void CanEvaluateExpressionNumberProperties()
        {
            var list = new List<INode>
                           {
                               new Number("px", 1),
                               new Operator("+"),
                               new Number("px", 2),
                               new Operator("*"),
                               new Number(20)
                           };
            var exp = new Expression(list);
            var prop = new Property("height", new[] {exp});

            Assert.That(prop.ToCss(), Is.EqualTo("height: 41px;"));

            var newNumber = prop.Evaluate();

            Assert.That(newNumber, Is.TypeOf<Number>());
        }

        [Test]
        public void CanEvaluateSeveralPropertiesWithoutOperators()
        {
            var list = new List<INode>
                           {
                               new Number("px", 1),
                               new Number("px", 2),
                               new Number("px", 3),
                               new Number("px", 4),
                           };
            var exp = new Expression(list);
            var prop = new Property("border", new[] {exp});

            Assert.That(prop.ToCss(), Is.EqualTo("border: 1px 2px 3px 4px;"));
        }
    }
}