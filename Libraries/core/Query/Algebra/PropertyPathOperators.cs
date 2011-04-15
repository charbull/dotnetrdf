﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public interface IPathOperator : ISparqlAlgebra
    {
        PatternItem PathStart
        {
            get;
        }

        PatternItem PathEnd
        {
            get;
        }

        ISparqlPath Path
        {
            get;
        }
    }

    public abstract class BasePathOperator : IPathOperator
    {
        private PatternItem _start, _end;
        private ISparqlPath _path;
        private HashSet<String> _vars = new HashSet<string>();

        public BasePathOperator(PatternItem start, PatternItem end, ISparqlPath path)
        {
            this._start = start;
            this._end = end;
            this._path = path;

            if (this._start.VariableName != null) this._vars.Add(this._start.VariableName);
            if (this._end.VariableName != null) this._vars.Add(this._end.VariableName);
        }

        public PatternItem PathStart
        {
            get 
            { 
                return this._start; 
            }
        }

        public PatternItem PathEnd
        {
            get 
            { 
                return this._end; 
            }
        }

        public ISparqlPath Path
        {
            get 
            { 
                return this._path;
            }
        }

        public abstract BaseMultiset Evaluate(SparqlEvaluationContext context);

        public IEnumerable<string> Variables
        {
            get 
            {
                return this._vars;
            }
        }

        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        public abstract GraphPattern ToGraphPattern();

        public abstract override string ToString();
    }

    public class ZeroLengthPath : BasePathOperator
    {
        public ZeroLengthPath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }
    
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            HashSet<KeyValuePair<INode,INode>> matches = new HashSet<KeyValuePair<INode,INode>>();

            if (context.Data.ActiveGraph != null)
            {
                this.GetMatches(context, context.Data.ActiveGraph, matches);
            }
            else if (context.Data.DefaultGraph != null)
            {
                this.GetMatches(context, context.Data.DefaultGraph, matches);
            }
            else
            {
                bool datasetOk = false;
                try
                {
                    foreach (Uri u in context.Data.GraphUris)
                    {
                        //This bit of logic takes care of the fact that calling SetActiveGraph((Uri)null) resets the
                        //Active Graph to be the default graph which if the default graph is null is usually the Union of
                        //all Graphs in the Store
                        if (u == null && context.Data.DefaultGraph == null && context.Data.UsesUnionDefaultGraph)
                        {
                            if (context.Data.HasGraph(null))
                            {
                                context.Data.SetActiveGraph(context.Data[null]);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            context.Data.SetActiveGraph(u);
                        }

                        datasetOk = true;

                        this.GetMatches(context, context.Data.ActiveGraph, matches);
                        context.Data.ResetActiveGraph();
                        datasetOk = false;
                    }
                }
                finally
                {
                    if (datasetOk) context.Data.ResetActiveGraph();
                }
            }

            if (matches.Count == 0)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                if (this.PathStart.VariableName == null && this.PathEnd.VariableName == null)
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
                else
                {
                    context.OutputMultiset = new Multiset();
                    String subjVar = this.PathStart.VariableName;
                    String objVar = this.PathEnd.VariableName;
                    foreach (KeyValuePair<INode, INode> m in matches)
                    {
                        Set s = new Set();
                        if (subjVar != null) s.Add(subjVar, m.Key);
                        if (objVar != null) s.Add(objVar, m.Value);
                        context.OutputMultiset.Add(s);
                    }
                }
            }
            return context.OutputMultiset;
        }

        private void GetMatches(SparqlEvaluationContext context, IGraph g, HashSet<KeyValuePair<INode, INode>> ms)
        {
            foreach (Triple t in g.Triples)
            {
                if (this.PathStart.Accepts(context, t.Subject) && this.PathEnd.Accepts(context, t.Object))
                {
                    ms.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
                }
            }
        }

        public override string  ToString()
        {
            return "ZeroLengthPath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new FixedCardinality(this.Path, 0), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class ZeroOrMorePath : BasePathOperator
    {
        public ZeroOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "ZeroOrMorePath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new ZeroOrMore(this.Path), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class OneOrMorePath : BasePathOperator
    {
        public OneOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "OneOrMorePath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new OneOrMore(this.Path), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }

    public class NegatedPropertySet : ISparqlAlgebra
    {
        private List<Property> _properties = new List<Property>();
        private PatternItem _start, _end;

        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties)
        {
            this._start = start;
            this._end = end;
            this._properties.AddRange(properties);
        }

        public PatternItem PathStart
        {
            get
            {
                return this._start;
            }
        }

        public PatternItem PathEnd
        {
            get
            {
                return this._end;
            }
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this._properties;
            }
        }

        #region ISparqlAlgebra Members

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Variables
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new VDS.RDF.Query.Paths.NegatedPropertySet(this._properties, Enumerable.Empty<Property>()), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("NegatedPropertySet(");
            output.Append(this._start.ToString());
            output.Append(", {");
            for (int i = 0; i < this._properties.Count; i++)
            {
                output.Append(this._properties[i].ToString());
                if (i < this._properties.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append("}, ");
            output.Append(this._end.ToString());
            output.Append(')');

            return output.ToString();
        }
    }

}
