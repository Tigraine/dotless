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

using System.Linq;
using dotless.Core.engine.CssNodes;
using dotless.Core.engine.Pipeline;

namespace dotless.Core.engine
{
    public class ExtensibleEngineImpl
    {
        private readonly string _source;
        private readonly ElementBlock _tail;

        public ElementBlock LessDom { get; private set; }

        private CssDocument _cssDom;
        public CssDocument CssDom
        {
            get
            {
                if(_cssDom == null)
                {
                    //Convert the LessDom to the CssDom and run any CSS Dom preprocessors set
                    _cssDom = PipelineFactory.LessToCssDomConverter.BuildCssDocument(LessDom);
                    _cssDom = RunCssDomPreprocessors(_cssDom);
                }
                return _cssDom;
            }
        }

        private string _css;
        public string Css
        {
            get
            {
                if(_css == null)
                {
                    //Convert the CssDom to Css
                    _css = PipelineFactory.CssBuilder.ToCss(CssDom);                 
                }
                return _css;
            }
        }

        public ExtensibleEngineImpl(string source, ElementBlock tail)
        {
          //Parse the source file and run any Less preprocessors set
          LessDom = PipelineFactory.LessParser.Parse(source, tail);
          LessDom = RunLessDomPreprocessors(LessDom);
        }

        /// <summary>
        /// New engine impl
        /// </summary>
        /// <param name="source"></param>
        public ExtensibleEngineImpl(string source) : this(source, null)
        {

        }

        /// <summary>
        /// Preprocess the Less document before it is sent to the Css converter
        /// </summary>
        /// <param name="lessDom"></param>
        private static ElementBlock RunLessDomPreprocessors(ElementBlock lessDom)
        {
            if (PipelineFactory.LessDomPreprocessors != null)
                lessDom = PipelineFactory.LessDomPreprocessors.Aggregate(lessDom, (current, lessPreprocessor) => lessPreprocessor.Process(current));
            
            return lessDom;
        }

        /// <summary>
        /// 
        /// Preprocessing CSS Dom before its converted to Css
        /// </summary>
        /// <param name="cssDom"></param>
        private static CssDocument RunCssDomPreprocessors(CssDocument cssDom)
        {
            if (PipelineFactory.CssDomPreprocessors != null)
                cssDom = PipelineFactory.CssDomPreprocessors.Aggregate(cssDom, (current, cssPreprocessor) => cssPreprocessor.Process(current));

            return cssDom;
        }

    }
}