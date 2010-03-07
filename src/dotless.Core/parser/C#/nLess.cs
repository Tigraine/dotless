/* created on 01/03/2010 01:42:25 from peg generator V1.0 using '' as input*/

using Peg.Base;
using System;
using System.IO;
using System.Text;
namespace nLess
{
      
      enum EnLess{Parse= 1, primary= 2, comment= 3, ruleset= 4, standard_ruleset= 5, 
                   hide= 6, mixin_ruleset= 7, mixin= 8, parameterized_mixin= 9, 
                   mixin_name= 10, mixin_arguments= 11, selector_mixin= 12, selectors= 13, 
                   selector= 14, parameters= 15, parameter= 16, import= 17, insert= 18, 
                   import_url= 19, url= 20, url_path= 21, medias= 22, declaration= 23, 
                   standard_declaration= 24, empty_declaration= 25, expressions= 26, 
                   operation_expressions= 27, space_delimited_expressions= 28, expression= 29, 
                   important= 30, ident= 31, variable= 32, element= 33, pseudo_exp= 34, 
                   attribute= 35, @class= 36, id= 37, tag= 38, select= 39, accessor= 40, 
                   accessor_name= 41, accessor_key= 42, @operator= 43, function= 44, 
                   function_name= 45, arguments= 46, entity= 47, fonts= 48, font= 49, 
                   literal= 50, keyword= 51, @string= 52, dimension= 53, number= 54, 
                   unit= 55, color= 56, rgb= 57, rgb_node= 58, hex= 59, alpha= 60, 
                   s= 61, S= 62, ws= 63, WS= 64, ns= 65, @as= 66};
      class nLess : PegCharParser 
      {
        
         #region Input Properties
        public static EncodingClass encodingClass = EncodingClass.ascii;
        public static UnicodeDetection unicodeDetection = UnicodeDetection.notApplicable;
        #endregion Input Properties
        #region Constructors
        public nLess()
            : base()
        {
            
        }
        public nLess(string src,TextWriter FerrOut)
			: base(src,FerrOut)
        {
            
        }
        #endregion Constructors
        #region Overrides
        public override string GetRuleNameFromId(int id)
        {
            try
            {
                   EnLess ruleEnum = (EnLess)id;
                    string s= ruleEnum.ToString();
                    int val;
                    if( int.TryParse(s,out val) ){
                        return base.GetRuleNameFromId(id);
                    }else{
                        return s;
                    }
            }
            catch (Exception)
            {
                return base.GetRuleNameFromId(id);
            }
        }
        public override void GetProperties(out EncodingClass encoding, out UnicodeDetection detection)
        {
            encoding = encodingClass;
            detection = unicodeDetection;
        } 
        #endregion Overrides
		#region Grammar Rules
        public bool Parse()    /*^Parse: primary;*/
        {

           return TreeAST((int)EnLess.Parse,()=> primary() );
		}
        public bool primary()    /*^^primary: 
                      (import / insert / declaration / ruleset / mixin / comment)*;*/
        {

           return TreeNT((int)EnLess.primary,()=>
                OptRepeat(()=>  
                      
                         import()
                      || insert()
                      || declaration()
                      || ruleset()
                      || mixin()
                      || comment() ) );
		}
        public bool comment()    /*^^comment:
                  ws '/*' (!'* /' . )* '* /' ws / ws '//' (!'\n' .)* '\n' ws;*/
        {

           return TreeNT((int)EnLess.comment,()=>
                  
                     And(()=>    
                         ws()
                      && Char('/','*')
                      && OptRepeat(()=>      
                            And(()=>    Not(()=> Char('*','/') ) && Any() ) )
                      && Char('*','/')
                      && ws() )
                  || And(()=>    
                         ws()
                      && Char('/','/')
                      && OptRepeat(()=>      
                            And(()=>    Not(()=> Char('\n') ) && Any() ) )
                      && Char('\n')
                      && ws() ) );
		}
        public bool ruleset()    /*^^ruleset:
                    standard_ruleset / mixin_ruleset;*/
        {

           return TreeNT((int)EnLess.ruleset,()=>
                    standard_ruleset() || mixin_ruleset() );
		}
        public bool standard_ruleset()    /*^^standard_ruleset:
                    selectors '{' ws primary ws '}' s hide ws;*/
        {

           return TreeNT((int)EnLess.standard_ruleset,()=>
                And(()=>  
                     selectors()
                  && Char('{')
                  && ws()
                  && primary()
                  && ws()
                  && Char('}')
                  && s()
                  && hide()
                  && ws() ) );
		}
        public bool hide()    /*^^hide:
                    ';'?;*/
        {

           return TreeNT((int)EnLess.hide,()=>
                Option(()=> Char(';') ) );
		}
        public bool mixin_ruleset()    /*^^mixin_ruleset:
                    ws mixin_name ws parameters ws '{' ws primary ws '}' ws;*/
        {

           return TreeNT((int)EnLess.mixin_ruleset,()=>
                And(()=>  
                     ws()
                  && mixin_name()
                  && ws()
                  && parameters()
                  && ws()
                  && Char('{')
                  && ws()
                  && primary()
                  && ws()
                  && Char('}')
                  && ws() ) );
		}
        public bool mixin()    /*^^mixin:
                    parameterized_mixin / selector_mixin;*/
        {

           return TreeNT((int)EnLess.mixin,()=>
                    parameterized_mixin() || selector_mixin() );
		}
        public bool parameterized_mixin()    /*^^parameterized_mixin:
                    mixin_name mixin_arguments s ';' ws;*/
        {

           return TreeNT((int)EnLess.parameterized_mixin,()=>
                And(()=>  
                     mixin_name()
                  && mixin_arguments()
                  && s()
                  && Char(';')
                  && ws() ) );
		}
        public bool mixin_name()    /*^^mixin_name:
                    '.' [-a-zA-Z0-9_]+;*/
        {

           return TreeNT((int)EnLess.mixin_name,()=>
                And(()=>  
                     Char('.')
                  && PlusRepeat(()=>    
                      (In('a','z', 'A','Z', '0','9')||OneOf("-_")) ) ) );
		}
        public bool mixin_arguments()    /*^^mixin_arguments:
                    '(' s ((parameter &as / expressions &as) (s ',' s (parameter &as / expressions &as))*)? s ')';*/
        {

           return TreeNT((int)EnLess.mixin_arguments,()=>
                And(()=>  
                     Char('(')
                  && s()
                  && Option(()=>    
                      And(()=>      
                               (        
                                       And(()=>    parameter() && Peek(()=> @as() ) )
                                    || And(()=>    expressions() && Peek(()=> @as() ) ))
                            && OptRepeat(()=>        
                                    And(()=>          
                                                 s()
                                              && Char(',')
                                              && s()
                                              && (            
                                                             And(()=>    parameter() && Peek(()=> @as() ) )
                                                          || And(()=>              
                                                                           expressions()
                                                                        && Peek(()=> @as() ) )) ) ) ) )
                  && s()
                  && Char(')') ) );
		}
        public bool selector_mixin()    /*^^selector_mixin:
                      ws selectors ';' ws;*/
        {

           return TreeNT((int)EnLess.selector_mixin,()=>
                And(()=>    ws() && selectors() && Char(';') && ws() ) );
		}
        public bool selectors()    /*^^selectors:
                      ws selector (s ',' ws selector)* ws;*/
        {

           return TreeNT((int)EnLess.selectors,()=>
                And(()=>  
                     ws()
                  && selector()
                  && OptRepeat(()=>    
                      And(()=>    s() && Char(',') && ws() && selector() ) )
                  && ws() ) );
		}
        public bool selector()    /*^^selector:
                      (s select element s)+;*/
        {

           return TreeNT((int)EnLess.selector,()=>
                PlusRepeat(()=>  
                  And(()=>    s() && select() && element() && s() ) ) );
		}
        public bool parameters()    /*^^parameters:
                      '(' s ')'
                      /
                      '(' parameter (s ',' s parameter)* ')'
                      ;*/
        {

           return TreeNT((int)EnLess.parameters,()=>
                  
                     And(()=>    Char('(') && s() && Char(')') )
                  || And(()=>    
                         Char('(')
                      && parameter()
                      && OptRepeat(()=>      
                            And(()=>    s() && Char(',') && s() && parameter() ) )
                      && Char(')') ) );
		}
        public bool parameter()    /*^^parameter:
                      variable s ':' s expressions;*/
        {

           return TreeNT((int)EnLess.parameter,()=>
                And(()=>  
                     variable()
                  && s()
                  && Char(':')
                  && s()
                  && expressions() ) );
		}
        public bool import()    /*^^import:
                      ws '@import' S import_url medias? s ';' ws;*/
        {

           return TreeNT((int)EnLess.import,()=>
                And(()=>  
                     ws()
                  && Char('@','i','m','p','o','r','t')
                  && S()
                  && import_url()
                  && Option(()=> medias() )
                  && s()
                  && Char(';')
                  && ws() ) );
		}
        public bool insert()    /*^^insert:
                      ws '@insert' S import_url medias? s ';' ws;*/
        {

           return TreeNT((int)EnLess.insert,()=>
                And(()=>  
                     ws()
                  && Char('@','i','n','s','e','r','t')
                  && S()
                  && import_url()
                  && Option(()=> medias() )
                  && s()
                  && Char(';')
                  && ws() ) );
		}
        public bool import_url()    /*^^import_url:
                  (string / url);*/
        {

           return TreeNT((int)EnLess.import_url,()=>
                    @string() || url() );
		}
        public bool url()    /*^^url:
                      'url(' url_path ')';*/
        {

           return TreeNT((int)EnLess.url,()=>
                And(()=>  
                     Char('u','r','l','(')
                  && url_path()
                  && Char(')') ) );
		}
        public bool url_path()    /*^^url_path:
                      (string / [-a-zA-Z0-9_%$/.&=:;#+?]+);*/
        {

           return TreeNT((int)EnLess.url_path,()=>
                  
                     @string()
                  || PlusRepeat(()=> OneOf(optimizedCharset0) ) );
		}
        public bool medias()    /*^^medias:
                  [-a-z]+ (s ',' s [a-z]+)*;*/
        {

           return TreeNT((int)EnLess.medias,()=>
                And(()=>  
                     PlusRepeat(()=> (In('a','z')||OneOf("-")) )
                  && OptRepeat(()=>    
                      And(()=>      
                               s()
                            && Char(',')
                            && s()
                            && PlusRepeat(()=> In('a','z') ) ) ) ) );
		}
        public bool declaration()    /*^^declaration:
                      standard_declaration / empty_declaration;*/
        {

           return TreeNT((int)EnLess.declaration,()=>
                    standard_declaration() || empty_declaration() );
		}
        public bool standard_declaration()    /*^^standard_declaration:
                      ws (ident / variable) s ':' ws expressions (ws ',' ws expressions)* s (';'/ ws &'}') ws;*/
        {

           return TreeNT((int)EnLess.standard_declaration,()=>
                And(()=>  
                     ws()
                  && (    ident() || variable())
                  && s()
                  && Char(':')
                  && ws()
                  && expressions()
                  && OptRepeat(()=>    
                      And(()=>      
                               ws()
                            && Char(',')
                            && ws()
                            && expressions() ) )
                  && s()
                  && (    
                         Char(';')
                      || And(()=>    ws() && Peek(()=> Char('}') ) ))
                  && ws() ) );
		}
        public bool empty_declaration()    /*^^empty_declaration:
                      ws ident s ':' s ';' ws;*/
        {

           return TreeNT((int)EnLess.empty_declaration,()=>
                And(()=>  
                     ws()
                  && ident()
                  && s()
                  && Char(':')
                  && s()
                  && Char(';')
                  && ws() ) );
		}
        public bool expressions()    /*^^expressions:
                      operation_expressions / space_delimited_expressions / [-a-zA-Z0-9_.&* /=:,+? \[\]()#%]+ ;*/
        {

           return TreeNT((int)EnLess.expressions,()=>
                  
                     operation_expressions()
                  || space_delimited_expressions()
                  || PlusRepeat(()=> OneOf(optimizedCharset1) ) );
		}
        public bool operation_expressions()    /*^^operation_expressions:
                      expression (operator expression)+;*/
        {

           return TreeNT((int)EnLess.operation_expressions,()=>
                And(()=>  
                     expression()
                  && PlusRepeat(()=>    
                      And(()=>    @operator() && expression() ) ) ) );
		}
        public bool space_delimited_expressions()    /*^^space_delimited_expressions:
                      expression (WS expression)* important?;*/
        {

           return TreeNT((int)EnLess.space_delimited_expressions,()=>
                And(()=>  
                     expression()
                  && OptRepeat(()=> And(()=>    WS() && expression() ) )
                  && Option(()=> important() ) ) );
		}
        public bool expression()    /*^^expression:
                      '(' s expressions s ')' / entity ;*/
        {

           return TreeNT((int)EnLess.expression,()=>
                  
                     And(()=>    
                         Char('(')
                      && s()
                      && expressions()
                      && s()
                      && Char(')') )
                  || entity() );
		}
        public bool important()    /*^^important:
                      s '!' s 'important';*/
        {

           return TreeNT((int)EnLess.important,()=>
                And(()=>    s() && Char('!') && s() && Char("important") ) );
		}
        public bool ident()    /*^^ident:
                  '*'? '-'? [-a-z_] [-a-z0-9_]*;*/
        {

           return TreeNT((int)EnLess.ident,()=>
                And(()=>  
                     Option(()=> Char('*') )
                  && Option(()=> Char('-') )
                  && (In('a','z')||OneOf("-_"))
                  && OptRepeat(()=> (In('a','z', '0','9')||OneOf("-_")) ) ) );
		}
        public bool variable()    /*^^variable:
                      '@' [-a-zA-Z0-9_]+;*/
        {

           return TreeNT((int)EnLess.variable,()=>
                And(()=>  
                     Char('@')
                  && PlusRepeat(()=>    
                      (In('a','z', 'A','Z', '0','9')||OneOf("-_")) ) ) );
		}
        public bool element()    /*^^element:
                 ( (class / id / tag / ident) attribute* ('(' [a-zA-Z]+ ')' / '(' (pseudo_exp / selector / [0-9]+) ')' )? )+
                 / attribute+ / '@media' / '@font-face'
                 ;*/
        {

           return TreeNT((int)EnLess.element,()=>
                  
                     PlusRepeat(()=>    
                      And(()=>      
                               (    @class() || id() || tag() || ident())
                            && OptRepeat(()=> attribute() )
                            && Option(()=>        
                                              
                                                 And(()=>            
                                                             Char('(')
                                                          && PlusRepeat(()=> In('a','z', 'A','Z') )
                                                          && Char(')') )
                                              || And(()=>            
                                                             Char('(')
                                                          && (              
                                                                           pseudo_exp()
                                                                        || selector()
                                                                        || PlusRepeat(()=> In('0','9') ))
                                                          && Char(')') ) ) ) )
                  || PlusRepeat(()=> attribute() )
                  || Char('@','m','e','d','i','a')
                  || Char("@font-face") );
		}
        public bool pseudo_exp()    /*pseudo_exp:
                  '-'? ([0-9]+)? 'n' ([-+] [0-9]+)?;*/
        {

           return And(()=>  
                     Option(()=> Char('-') )
                  && Option(()=> PlusRepeat(()=> In('0','9') ) )
                  && Char('n')
                  && Option(()=>    
                      And(()=>      
                               OneOf("-+")
                            && PlusRepeat(()=> In('0','9') ) ) ) );
		}
        public bool attribute()    /*^^attribute:
                  '[' tag ([|~*$^]? '=') (string / [-a-zA-Z_0-9]+) ']' / '[' (tag / string) ']';*/
        {

           return TreeNT((int)EnLess.attribute,()=>
                  
                     And(()=>    
                         Char('[')
                      && tag()
                      && And(()=>      
                               Option(()=> OneOf("|~*$^") )
                            && Char('=') )
                      && (      
                               @string()
                            || PlusRepeat(()=>        
                                    (In('a','z', 'A','Z', '0','9')||OneOf("-_")) ))
                      && Char(']') )
                  || And(()=>    
                         Char('[')
                      && (    tag() || @string())
                      && Char(']') ) );
		}
        public bool @class()    /*^^class:
                  '.' [_a-zA-Z] [-a-zA-Z0-9_]*;*/
        {

           return TreeNT((int)EnLess.@class,()=>
                And(()=>  
                     Char('.')
                  && (In('a','z', 'A','Z')||OneOf("_"))
                  && OptRepeat(()=>    
                      (In('a','z', 'A','Z', '0','9')||OneOf("-_")) ) ) );
		}
        public bool id()    /*^^id:
                  '#' [_a-zA-Z] [-a-zA-Z0-9_]*;*/
        {

           return TreeNT((int)EnLess.id,()=>
                And(()=>  
                     Char('#')
                  && (In('a','z', 'A','Z')||OneOf("_"))
                  && OptRepeat(()=>    
                      (In('a','z', 'A','Z', '0','9')||OneOf("-_")) ) ) );
		}
        public bool tag()    /*^^tag:
                  [a-zA-Z] [-a-zA-Z]* [0-9]? / '*';*/
        {

           return TreeNT((int)EnLess.tag,()=>
                  
                     And(()=>    
                         In('a','z', 'A','Z')
                      && OptRepeat(()=> (In('a','z', 'A','Z')||OneOf("-")) )
                      && Option(()=> In('0','9') ) )
                  || Char('*') );
		}
        public bool select()    /*^^select:
                  (s [+>~] s / '::' / s ':' / S)?;*/
        {

           return TreeNT((int)EnLess.select,()=>
                Option(()=>  
                      
                         And(()=>    s() && OneOf("+>~") && s() )
                      || Char(':',':')
                      || And(()=>    s() && Char(':') )
                      || S() ) );
		}
        public bool accessor()    /*^^accessor:
                      accessor_name '[' accessor_key ']';*/
        {

           return TreeNT((int)EnLess.accessor,()=>
                And(()=>  
                     accessor_name()
                  && Char('[')
                  && accessor_key()
                  && Char(']') ) );
		}
        public bool accessor_name()    /*^^accessor_name:
                      (class / id / tag);*/
        {

           return TreeNT((int)EnLess.accessor_name,()=>
                    @class() || id() || tag() );
		}
        public bool accessor_key()    /*^^accessor_key:
                      (string / variable);*/
        {

           return TreeNT((int)EnLess.accessor_key,()=>
                    @string() || variable() );
		}
        public bool @operator()    /*^^operator:
                      S [-+* /] S / [-+* /] ;*/
        {

           return TreeNT((int)EnLess.@operator,()=>
                  
                     And(()=>    S() && OneOf("-+*/") && S() )
                  || OneOf("-+*/") );
		}
        public bool function()    /*^^function:
                      function_name arguments;*/
        {

           return TreeNT((int)EnLess.function,()=>
                And(()=>    function_name() && arguments() ) );
		}
        public bool function_name()    /*^^function_name:
                      [-a-zA-Z_]+;*/
        {

           return TreeNT((int)EnLess.function_name,()=>
                PlusRepeat(()=> (In('a','z', 'A','Z')||OneOf("-_")) ) );
		}
        public bool arguments()    /*^^arguments:
                      '(' s expressions s (',' s expressions s)* ')'
                      /
                      '(' s ')'
                      ;*/
        {

           return TreeNT((int)EnLess.arguments,()=>
                  
                     And(()=>    
                         Char('(')
                      && s()
                      && expressions()
                      && s()
                      && OptRepeat(()=>      
                            And(()=>        
                                       Char(',')
                                    && s()
                                    && expressions()
                                    && s() ) )
                      && Char(')') )
                  || And(()=>    Char('(') && s() && Char(')') ) );
		}
        public bool entity()    /*^^entity:
                      url / alpha / function / accessor / keyword / variable / literal / font;*/
        {

           return TreeNT((int)EnLess.entity,()=>
                  
                     url()
                  || alpha()
                  || function()
                  || accessor()
                  || keyword()
                  || variable()
                  || literal()
                  || font() );
		}
        public bool fonts()    /*^^fonts:
                        font (s ',' s font)+;*/
        {

           return TreeNT((int)EnLess.fonts,()=>
                And(()=>  
                     font()
                  && PlusRepeat(()=>    
                      And(()=>    s() && Char(',') && s() && font() ) ) ) );
		}
        public bool font()    /*^^font:
                      [a-zA-Z] [-a-zA-Z0-9]* !ns
                      / 
                      string
                      ;*/
        {

           return TreeNT((int)EnLess.font,()=>
                  
                     And(()=>    
                         In('a','z', 'A','Z')
                      && OptRepeat(()=>      
                            (In('a','z', 'A','Z', '0','9')||OneOf("-")) )
                      && Not(()=> ns() ) )
                  || @string() );
		}
        public bool literal()    /*^^literal:
                      color / (dimension / [-a-z]+) '/' dimension / number unit / string;*/
        {

           return TreeNT((int)EnLess.literal,()=>
                  
                     color()
                  || And(()=>    
                         (      
                               dimension()
                            || PlusRepeat(()=> (In('a','z')||OneOf("-")) ))
                      && Char('/')
                      && dimension() )
                  || And(()=>    number() && unit() )
                  || @string() );
		}
        public bool keyword()    /*^^keyword:
                      [-a-zA-Z]+ !ns;*/
        {

           return TreeNT((int)EnLess.keyword,()=>
                And(()=>  
                     PlusRepeat(()=> (In('a','z', 'A','Z')||OneOf("-")) )
                  && Not(()=> ns() ) ) );
		}
        public bool @string()    /*^^string:
                  ['] (!['] . )* [']
                  /
                  ["] (!["] . )* ["]
                  ;*/
        {

           return TreeNT((int)EnLess.@string,()=>
                  
                     And(()=>    
                         OneOf("'")
                      && OptRepeat(()=>      
                            And(()=>    Not(()=> OneOf("'") ) && Any() ) )
                      && OneOf("'") )
                  || And(()=>    
                         OneOf("\"")
                      && OptRepeat(()=>      
                            And(()=>    Not(()=> OneOf("\"") ) && Any() ) )
                      && OneOf("\"") ) );
		}
        public bool dimension()    /*^^dimension:
                  number unit;*/
        {

           return TreeNT((int)EnLess.dimension,()=>
                And(()=>    number() && unit() ) );
		}
        public bool number()    /*^^number:
                      '-'? [0-9]* '.' [0-9]+ / '-'? [0-9]+;*/
        {

           return TreeNT((int)EnLess.number,()=>
                  
                     And(()=>    
                         Option(()=> Char('-') )
                      && OptRepeat(()=> In('0','9') )
                      && Char('.')
                      && PlusRepeat(()=> In('0','9') ) )
                  || And(()=>    
                         Option(()=> Char('-') )
                      && PlusRepeat(()=> In('0','9') ) ) );
		}
        public bool unit()    /*^^unit:
                      ('px'/'em'/'pc'/'%'/'ex'/'in'/'deg'/'s'/'pt'/'cm'/'mm')?;*/
        {

           return TreeNT((int)EnLess.unit,()=>
                Option(()=> OneOfLiterals(optimizedLiterals0) ) );
		}
        public bool color()    /*^^color:
                      '#' rgb;*/
        {

           return TreeNT((int)EnLess.color,()=>
                And(()=>    Char('#') && rgb() ) );
		}
        public bool rgb()    /*^^rgb:
                      rgb_node rgb_node rgb_node
                      /
                      hex hex hex;*/
        {

           return TreeNT((int)EnLess.rgb,()=>
                  
                     And(()=>    rgb_node() && rgb_node() && rgb_node() )
                  || And(()=>    hex() && hex() && hex() ) );
		}
        public bool rgb_node()    /*^^rgb_node:
                  hex hex;*/
        {

           return TreeNT((int)EnLess.rgb_node,()=>
                And(()=>    hex() && hex() ) );
		}
        public bool hex()    /*^^hex:
                  [a-fA-F0-9];*/
        {

           return TreeNT((int)EnLess.hex,()=>
                In('a','f', 'A','F', '0','9') );
		}
        public bool alpha()    /*^^alpha:
                      'alpha' s '(' s 'opacity=' variable ')';*/
        {

           return TreeNT((int)EnLess.alpha,()=>
                And(()=>  
                     Char('a','l','p','h','a')
                  && s()
                  && Char('(')
                  && s()
                  && Char("opacity=")
                  && variable()
                  && Char(')') ) );
		}
        public bool s()    /*s: [ ]*;*/
        {

           return OptRepeat(()=> OneOf(" ") );
		}
        public bool S()    /*S: [ ]+;*/
        {

           return PlusRepeat(()=> OneOf(" ") );
		}
        public bool ws()    /*ws: [\n\r\t ]*;*/
        {

           return OptRepeat(()=> OneOf("\n\r\t ") );
		}
        public bool WS()    /*WS: [\n\r\t ]+;*/
        {

           return PlusRepeat(()=> OneOf("\n\r\t ") );
		}
        public bool ns()    /*ns: ![ ;,!})\n\r] .;*/
        {

           return And(()=>    Not(()=> OneOf(optimizedCharset2) ) && Any() );
		}
        public bool @as()    /*as: s [,)];*/
        {

           return And(()=>    s() && OneOf(",)") );
		}
		#endregion Grammar Rules

        #region Optimization Data 
        internal static OptimizedCharset optimizedCharset0;
        internal static OptimizedCharset optimizedCharset1;
        internal static OptimizedCharset optimizedCharset2;
        
        internal static OptimizedLiterals optimizedLiterals0;
        
        static nLess()
        {
            {
               OptimizedCharset.Range[] ranges = new OptimizedCharset.Range[]
                  {new OptimizedCharset.Range('a','z'),
                   new OptimizedCharset.Range('A','Z'),
                   new OptimizedCharset.Range('0','9'),
                   };
               char[] oneOfChars = new char[]    {'-','_','%','$','/'
                                                  ,'.','&','=',':',';'
                                                  ,'#','+','?'};
               optimizedCharset0= new OptimizedCharset(ranges,oneOfChars);
            }
            
            {
               OptimizedCharset.Range[] ranges = new OptimizedCharset.Range[]
                  {new OptimizedCharset.Range('a','z'),
                   new OptimizedCharset.Range('A','Z'),
                   new OptimizedCharset.Range('0','9'),
                   };
               char[] oneOfChars = new char[]    {'-','_','.','&','*'
                                                  ,'/','=',':',',','+'
                                                  ,'?',' ','\\','[',']'
                                                  ,'(',')','#','%'};
               optimizedCharset1= new OptimizedCharset(ranges,oneOfChars);
            }
            
            {
               char[] oneOfChars = new char[]    {' ',';',',','!','}'
                                                  ,')','\n','\r'};
               optimizedCharset2= new OptimizedCharset(null,oneOfChars);
            }
            
            
            {
               string[] literals=
               { "px","em","pc","%","ex","in","deg","s",
                  "pt","cm","mm" };
               optimizedLiterals0= new OptimizedLiterals(literals);
            }

            
        }
        #endregion Optimization Data 
           }
}