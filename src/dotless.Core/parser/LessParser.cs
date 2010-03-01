﻿/* Copyright 2009 dotless project, http://www.dotlesscss.com
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

using System;
using dotless.Core.engine;
using dotless.Core.exceptions;
using Peg.Base;

namespace dotless.Core.parser
{
    public class LessParser : ILessParser
    {
        public ElementBlock Parse(string source)
        {
            return Parse(source, null);
        }

        public ElementBlock Parse(string source, ElementBlock tail)
        {
            var parser = new nLess.LessImpl(source, Console.Out);
            if (!parser.Parse()) throw new ParsingException("FAILURE: Parser did not match input file");
            return new TreeBuilder(parser.GetRoot(), source).Build(tail);
        }
    }


    public class LessTreePrinterParser : ILessParser
    {
        public ElementBlock Parse(string source)
        {
            return Parse(source, null);
        }

        public ElementBlock Parse(string source, ElementBlock tail)
        {
            var parser = new nLess.LessImpl(source, Console.Out);
            if (!parser.Parse()) throw new ParsingException("FAILURE: Parser did not match input file");
            new TreePrint(Console.Out, source, 60, new NodePrinter(parser).GetNodeName, false)
                .PrintTree(parser.GetRoot(), 0, 0);
            return new TreeBuilder(parser.GetRoot(), source).Build(tail);
        }
    }
}