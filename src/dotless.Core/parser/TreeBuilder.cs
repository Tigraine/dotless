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
using dotless.Core.utils;

namespace dotless.Core.parser
{
    using engine;
    using exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using nLess;
    using String=engine.String;

    internal class TreeBuilder
    {
        public LessPegRootNode Root { get; set; }
        public string Src { get; set; }

        public TreeBuilder(LessPegRootNode root, string src)
        {
            Root = root;
            Src = src;
        }
        
        /// <summary>
        /// Main entry point for the build
        /// </summary>
        /// <returns></returns>
        public ElementBlock Build()
        {
            return BuildPrimary(Root.Child);
        }

        /// <summary>
        /// Main entry point for the build
        /// </summary>
        /// <returns></returns>
        public ElementBlock Build(ElementBlock tail)
        {
            return tail == null ? Build() : BuildPrimary(Root.Child, tail);
        }

        public static ElementBlock BuildPrimary(LessPegNode node)
        {
            return BuildPrimary(node, new ElementBlock("*"));
        }

        //> <<Grammar Name="nLess">>

        //> ^Parse: primary;

        public static ElementBlock BuildPrimary(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.primary);
            //> ^^primary: 
            //>       (import / insert / declaration / ruleset / mixin / comment)*;

            node.Children().ToList().ForEach(n => BuildPrimaryNode(n, parent));

            return parent;
        }

        //> ^^comment:
        //>       ws '/*' (!'*/' . )* '*/' ws / ws '//' (!'\n' .)* '\n' ws;

        private static void BuildPrimaryNode(LessPegNode node, ElementBlock parent)
        {
            switch (node.Type())
            {
                case EnLess.import:
                    BuildImport(node, parent);
                    break;

                case EnLess.insert:
                    BuildInsert(node, parent);
                    break;

                case EnLess.declaration:
                    BuildDeclaration(node, parent);
                    break;
                
                case EnLess.ruleset:
                    BuildRuleset(node, parent);
                    break;
                
                case EnLess.mixin:
                    BuildMixin(node, parent);
                    break;

                case EnLess.comment:
                    break;

                default:
                    throw new ParsingException(string.Format("Unexpected node '{0}'", node.Type()));

            }
        }

        //
        // div, .class, body > p {...}
        //
        public static void BuildRuleset(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.ruleset);
            //> ^^ruleset:
            //>     standard_ruleset / mixin_ruleset;

            var ruleset = node.Child;

            if(ruleset.Type() == EnLess.standard_ruleset)
                StandardRuleset(ruleset, parent);

            if(ruleset.Type() == EnLess.mixin_ruleset)
                MixinRuleset(ruleset, parent);
        }

        public static void StandardRuleset(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.standard_ruleset);
            //> ^^standard_ruleset:
            //>     selectors '{' ws primary ws '}' s hide ws;

            //> ^^hide:
            //>     ';'?;

            var selectors = node.Child;
            var primary = selectors.Next;
            var hide = primary.Next;

            foreach (var selector in BuildSelectors(selectors, parent, GetRulesetSelector).SelectMany( e => e ))
            {
                selector.Hide = !hide.IsEmpty();
                BuildPrimary(primary, selector);
            }
        }

        public static void MixinRuleset(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.mixin_ruleset);
            // Mixin Declaration
            //> ^^mixin_ruleset:
            //>     ws mixin_name ws parameters ws '{' ws primary ws '}' ws;

            
            var name = node.Child;
            var parameters = name.Next;
            var primary = parameters.Next;

            var mixinBlock = new MixinBlock(name.TextValue, BuildParameters(parameters, parent));
            parent.Add(mixinBlock);

            BuildPrimary(primary, mixinBlock);
        }


        public static void BuildMixin(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.mixin);
            //> ^^mixin:
            //>     parameterized_mixin / selector_mixin;

            var mixin = node.Child;

            if (mixin.Type() == EnLess.parameterized_mixin)
                ParameterizedMixin(mixin, parent);

            if (mixin.Type() == EnLess.selector_mixin)
                SelectorMixin(mixin, parent);
        }

        // TODO: merge parameterized mixin and selector mixin to allow parameterized mixins inside namespaces!
        private static void ParameterizedMixin(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.parameterized_mixin);
            //> ^^parameterized_mixin:
            //>     mixin_name mixin_arguments s ';' ws;

            //> ^^mixin_name:
            //>     '.' [-a-zA-Z0-9_]+;

            //> ^^mixin_arguments:
            //>     '(' s ((parameter &as / expressions &as) (s ',' s (parameter &as / expressions &as))*)? s ')';

            var name = node.Child;
            var arguments = name.Next;

            var definition = parent.NearestAs<MixinBlock>(name.TextValue);

            if (definition == null)
                throw new ParsingException(string.Format("Unable to find mixin '{0}' in {1}", name.TextValue, parent.Name));

            var args = arguments.Children()
                .Select((a, i) => a.Type() == EnLess.expressions
                                      ? new Variable(i.ToString(), BuildExpressions(a, parent))
                                      : BuildParameter(a, parent));

            var rules = definition.GetClonedChildren(parent, args);

            parent.Children.AddRange(rules);
        }

        private static void SelectorMixin(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.selector_mixin);
            //> ^^selector_mixin:
            //>       ws selectors ';' ws;

            var selectors = BuildSelectors(node.Child, parent, GetMixinSelector);

            foreach (var path in selectors)
            {
                ElementBlock block;
                try
                {
                    block = path.Aggregate(parent.GetRoot(), (current, n) => current.Descend(n.Selector, n));
                }
                catch(NullReferenceException)
                {
                    throw new ParsingException(
                        string.Format(
                            "Unable to find mixin '{0}' in {1}",
                            node.Child.TextValue, parent.Name));
                }

                var rules = block is MixinBlock ? ((MixinBlock)block).GetClonedChildren(parent, null) : block.Children;

                parent.Children.AddRange(rules);
            }
        }

        public static IEnumerable<IEnumerable<ElementBlock>> BuildSelectors(LessPegNode node, ElementBlock parent, Func<LessPegNode, ElementBlock, IEnumerable<ElementBlock>> getSelectors)
        {
            Guard.ExpectPegNode(node, EnLess.selectors);
            //> ^^selectors:
            //>       ws selector (s ',' ws selector)* ws;

            return node.Children().Select(e => getSelectors(e, parent));
            // .Where( e => e != null );
        }

        public static IEnumerable<ElementBlock> GetRulesetSelector(LessPegNode node, ElementBlock parent)
        {
            var last = GetSelectorParts(node)
                .Aggregate(parent, (block, sel) =>
                                       {
                                           block.Add(sel);
                                           return sel;
                                       });
            yield return last;
        }

        public static IEnumerable<ElementBlock> GetMixinSelector(LessPegNode node, ElementBlock parent)
        {
            return GetSelectorParts(node);
        }

        //
        // div > p a {...}
        //
        private static IEnumerable<ElementBlock> GetSelectorParts(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.selector);
            //> ^^selector:
            //>       (s select element s)+;


            var enumerator = node.Children().GetEnumerator();
            while(enumerator.MoveNext())
            {
                var select = enumerator.Current.TextValue.Trim();

                if(!enumerator.MoveNext())
                    // throw? should never get here
                    yield break;

                var element = enumerator.Current.TextValue.Trim();

                yield return new ElementBlock(element, select);
            }
        }

        public static IEnumerable<Variable> BuildParameters(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.parameters);
            //> ^^parameters:
            //>       '(' s ')'
            //>       /
            //>       '(' parameter (s ',' s parameter)* ')'
            //>       ;

            return node.Children().Select(p => BuildParameter(p, parent));
        }

        public static Variable BuildParameter(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.parameter);
            //> ^^parameter:
            //>       variable s ':' s expressions;

            var variable = node.Child;
            var expressions = variable.Next;

            return new Variable(variable.TextValue, BuildExpressions(expressions, parent), parent);
        }

        public static void BuildImport(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.import);
            //> ^^import:
            //>       ws '@import' S import_url medias? s ';' ws;

            var importUrl = node.Child;
            var medias = importUrl.Next;

            if(importUrl.Child.Type() == EnLess.url)
                importUrl = importUrl.Child.Child;

            var path = importUrl.TextValue
                .Replace("\"", "").Replace("'", "");

            if (HttpContext.Current != null)
            {
                path = HttpContext.Current.Server.MapPath(path);
            }

            if (!File.Exists(path) && Path.GetExtension(path) != ".less")
                path = path + ".less";

            if (File.Exists(path))
            {
                var source = File.ReadAllText(path);
                var sourceDirectory = Path.GetDirectoryName(path);
                var currentDirectory = Directory.GetCurrentDirectory();

                if(!string.IsNullOrEmpty(sourceDirectory)) Directory.SetCurrentDirectory(sourceDirectory);
                
                new ExtensibleEngineImpl(source, parent);

                if (!string.IsNullOrEmpty(sourceDirectory)) Directory.SetCurrentDirectory(currentDirectory);
            }
        }

        public static void BuildInsert(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.insert);
            //> ^^insert:
            //>       ws '@insert' S import_url medias? s ';' ws;

            // TODO: Create a failing test before implementing BuildInsert
        }

        //> ^^import_url:
        //>       (string / url);

        public static INode BuildUrl(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.url);
            //> ^^url:
            //>       'url(' url_path ')';

            //> ^^url_path:
            //>       (string / [-a-zA-Z0-9_%$/.&=:;#+?]+);

            var path = node.Child.TextValue;

            return new Function("url", new[] {new String(path)});
        }

        //> ^^medias:
        //>       [-a-z]+ (s ',' s [a-z]+)*;

        //
        // @my-var: 12px;
        // height: 100%;
        //
        public static void BuildDeclaration(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.declaration);
            //> ^^declaration:
            //>       standard_declaration / empty_declaration;

            //> ^^standard_declaration:
            //>       ws (ident / variable) s ':' ws expressions (ws ',' ws expressions)* s (';'/ ws &'}') ws;

            //> ^^empty_declaration:
            //>       ws ident s ':' s ';' ws;

            var declaration = node.Child;

            if (declaration.Type() == EnLess.empty_declaration)
            {
                // TODO: throw? warn? ignore?
                parent.Add(new Anonymous(declaration.TextValue));
                return;
            }

            var ident = declaration.Child;

            var name = ident.TextValue;
            var expressions = ident.Next.Siblings();

            var result = expressions.Select(e => BuildExpressions(e, parent));

            if (ident.Type() == EnLess.variable)
                parent.Add(new Variable(name, result, parent));
            else
                parent.Add(new Property(name, result, parent));
        }

        //
        // An operation or compound value
        //
        public static INode BuildExpressions(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.expressions);
            //> ^^expressions:
            //>       operation_expressions / space_delimited_expressions / [-a-zA-Z0-9_.&*/=:,+? \[\]()#%]+ ;

            //> ^^operation_expressions:
            //>       expression (operator expression)+;

            //> ^^space_delimited_expressions:
            //>       expression (WS expression)* important?;

            var expression = node.Child;

            if (expression == null)
                return new Anonymous(node.TextValue);

            var nodes = expression.Children().Select(e => BuildExpressionNode(e, parent));

            return new Expression(nodes, parent);
        }

        private static INode BuildExpressionNode(LessPegNode node, ElementBlock parent)
        {
            // an expression node - operator, expression, or important

            if (node.Type() == EnLess.@operator)
                return BuildOperator(node);
            
            if (node.Type() == EnLess.important)
                return BuildImportant(node);

            return BuildExpression(node, parent);
        }

        private static INode BuildExpression(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.expression);
            //> ^^expression:
            //>       '(' s expressions s ')' / entity ;

            var expression = node.Child;

            if (expression.Type() == EnLess.expressions)
                return BuildExpressions(expression, parent);

            return BuildEntity(expression, parent);
        }

        // !important
        public static INode BuildImportant(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.important);
            //> ^^important:
            //>       s '!' s 'important';

            return new Keyword(node.TextValue.Trim());
        }

        //
        // An identifier
        //
        //> ^^ident:
        //>       '*'? '-'? [-a-z_] [-a-z0-9_]*;

        public static Variable BuildVariable(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.variable);
            //> ^^variable:
            //>       '@' [-a-zA-Z0-9_]+;

            return new Variable(node.TextValue, new INode[]{}, parent);
        }
        
        //
        // div / .class / #id / input[type="text"] / lang(fr)
        //
        //> ^^element:
        //>      ( (class / id / tag / ident) attribute* ('(' [a-zA-Z]+ ')' / '(' (pseudo_exp / selector / [0-9]+) ')' )? )+
        //>      / attribute+ / '@media' / '@font-face'
        //>      ;
        
        //
        // 4n+1
        //
        //> pseudo_exp:
        //>       '-'? ([0-9]+)? 'n' ([-+] [0-9]+)?;

        //
        // [type="text"]
        //
        //> ^^attribute:
        //>       '[' tag ([|~*$^]? '=') (string / [-a-zA-Z_0-9]+) ']' / '[' (tag / string) ']';

        //> ^^class:
        //>       '.' [_a-zA-Z] [-a-zA-Z0-9_]*;

        //> ^^id:
        //>       '#' [_a-zA-Z] [-a-zA-Z0-9_]*;

        //> ^^tag:
        //>       [a-zA-Z] [-a-zA-Z]* [0-9]? / '*';

        //> ^^select:
        //>       (s [+>~] s / '::' / s ':' / S)?;
        
        public static INode BuildAccessor(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.accessor);
            //> ^^accessor:
            //>       accessor_name '[' accessor_key ']';
            
            //> ^^accessor_name:
            //>       (class / id / tag);
            
            //> ^^accessor_key:
            //>       (string / variable);

            var accessorName = node.Child;
            var accessorKey = accessorName.Next;

            var name = accessorName.TextValue;
            var key = accessorKey.TextValue.Replace("'", "").Replace("\"", "");

            var el = parent.NearestAs<ElementBlock>(name);
            if (el != null)
            {
                var prop = el.GetAs<Property>(key);
                if (prop != null) return prop.Value;
            }
            return new Anonymous("");
        }

        public static INode BuildOperator(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.@operator);
            //> ^^operator:
            //>       S [-+*/] S / [-+*/] ;

            return new Operator(node.TextValue.Trim());
        }

        //
        // Functions and arguments
        //
        public static INode BuildFunction(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.function);
            //> ^^function:
            //>       function_name arguments;

            //> ^^function_name:
            //>       [-a-zA-Z_]+;

            var name = node.Child;
            var arguments = name.Next;

            return new Function(name.TextValue, BuildArguments(arguments, parent));
        }

        private static IEnumerable<INode> BuildArguments(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.arguments);
            //> ^^arguments:
            //>       '(' s expressions s (',' s expressions s)* ')'
            //>       /
            //>       '(' s ')'
            //>       ;

            return node.Children().Select(e => BuildExpressions(e, parent));
        }

        //*** Entity ***//

        //
        // Entity: Any whitespace delimited token
        //
        private static INode BuildEntity(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.entity);
            //> ^^entity:
            //>       url / alpha / function / accessor / keyword / variable / literal / font;

            var entity = node.Child;

            switch (entity.Type())
            {
                case EnLess.url:
                    return BuildUrl(entity);

                case EnLess.alpha:
                    return BuildAlpha(entity, parent);

                case EnLess.function:
                    return BuildFunction(entity, parent);

                case EnLess.accessor:
                    return BuildAccessor(entity, parent);

                case EnLess.keyword:
                    return BuildKeyword(entity);

                case EnLess.variable:
                    return BuildVariable(entity, parent);

                case EnLess.literal:
                    return BuildLiteral(entity);

                case EnLess.font:
                    return BuildFont(entity);

                default:
                    return new Anonymous(entity.TextValue);

            }
        }

        //private static INode BuildFonts(LessPegNode node)
        //{
        //    Guard.ExpectPegNode(node, EnLess.fonts);
        //    //> ^^fonts:
        //    //>       font (s ',' s font)+;
        //
        //    var fonts = node.Children()
        //        .Select( f => f.TextValue);
        //
        //    return new FontFamily(fonts.ToArray());
        //}

        private static INode BuildFont(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.font);
            //> ^^font:
            //>       [a-zA-Z] [-a-zA-Z0-9]* !ns
            //>       / 
            //>       string
            //>       ;

            var value = node.TextValue;

            if (node.Child != null)
                return new String(value);

            return new Keyword(value);
        }

        //
        // Tokens which don't need to be evaluated
        //
        private static INode BuildLiteral(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.literal);
            //> ^^literal:
            //>       color / (dimension / [-a-z]+) '/' dimension / number unit / string;

            var literal = node.Child;

            switch (literal.Type())
            {
                case EnLess.color:
                    return BuildColor(literal);

                case EnLess.number:
                    return BuildNumber(literal);

                case EnLess.@string:
                    return new String(literal.TextValue);

                default:
                    return new Anonymous(node.TextValue);
            }
        }


        //
        // `blue`, `small`, `normal` etc.
        //
        private static Keyword BuildKeyword(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.keyword);
            //> ^^keyword:
            //>       [-a-zA-Z]+ !ns;

            return new Keyword(node.TextValue);
        }

        //
        // 'hello world' / "hello world"
        //
        //> ^^string:
        //>       ['] (!['] . )* [']
        //>       /
        //>       ["] (!["] . )* ["]
        //>       ;

        //
        // Numbers & Units
        //
        //> ^^dimension:
        //>       number unit;

        private static INode BuildNumber(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.number);
            //> ^^number:
            //>       '-'? [0-9]* '.' [0-9]+ / '-'? [0-9]+;

            //> ^^unit:
            //>       ('px'/'em'/'pc'/'%'/'ex'/'in'/'deg'/'s'/'pt'/'cm'/'mm')?;

            var val = float.Parse(node.TextValue, NumberFormatInfo.InvariantInfo);
            var unit = "";
            node = node.Next;
            
            if (node != null && node.Type() == EnLess.unit) 
                unit = node.TextValue;
            
            return new Number(unit, val);
        }

        //
        // Color
        //
        public static Color BuildColor(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.color);
            //> ^^color:
            //>       '#' rgb;

            var rgb = BuildRgb(node.Child);

            return new Color(rgb);
        }

        //
        // 00ffdd / 0fd
        //
        public static int BuildRgb(LessPegNode node)
        {
            Guard.ExpectPegNode(node, EnLess.rgb);
            //> ^^rgb:
            //>       rgb_node rgb_node rgb_node
            //>       /
            //>       hex hex hex;

            var value = node.TextValue;

            if(value.Length == 3)
                value = new string(new[] { value[0], value[0], value[1], value[1], value[2], value[2] });

            return int.Parse(value, NumberStyles.HexNumber);
        }

        //> ^^rgb_node:
        //>       hex hex;

        //> ^^hex:
        //>       [a-fA-F0-9];

        //
        // Special case for IE alpha filter
        //
        public static INode BuildAlpha(LessPegNode node, ElementBlock parent)
        {
            Guard.ExpectPegNode(node, EnLess.alpha);
            //> ^^alpha:
            //>       'alpha' s '(' s 'opacity=' variable ')';

            var variable = BuildVariable(node.Child, parent);

            return new String(node.TextValue.Replace(variable.Name, variable.Evaluate().ToCss()));
        }


        //*** Common ***//

        //
        // Whitespace
        //
        //> s: [ ]*;
        //> S: [ ]+;
        //> ws: [\n\r\t ]*;
        //> WS: [\n\r\t ]+;

        // Non-space char
        //> ns: ![ ;,!})\n\r] .;
        
        // Argument separator
        //> as: s [,)];
        
        //> <</Grammar>>
    }
}