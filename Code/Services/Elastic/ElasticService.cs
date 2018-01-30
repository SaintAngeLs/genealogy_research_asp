﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Page = Bonsai.Data.Models.Page;

namespace Bonsai.Code.Services.Elastic
{
    /// <summary>
    /// The low-level service for working with ElasticSearch.
    /// </summary>
    public class ElasticService
    {
        public ElasticService(ElasticClient client)
        {
            _client = client;
        }

        private readonly ElasticClient _client;

        private const string PAGE_INDEX = "pages";
        private const string STOP_WORDS = "а,без,более,бы,был,была,были,было,быть,в,вам,вас,весь,во,вот,все,всего,всех,вы,где,да,даже,для,до,его,ее,если,есть,еще,же,за,здесь,и,из,или,им,их,к,как,ко,когда,кто,ли,либо,мне,может,мы,на,надо,наш,не,него,нее,нет,ни,них,но,ну,о,об,однако,он,она,они,оно,от,очень,по,под,при,с,со,так,также,такой,там,те,тем,то,того,тоже,той,только,том,ты,у,уже,хотя,чего,чей,чем,что,чтобы,чье,чья,эта,эти,это,я";

        #region Initialization

        /// <summary>
        /// Removes all cached data.
        /// </summary>
        public void ClearPreviousData()
        {
            if (_client.IndexExists(PAGE_INDEX).Exists)
                _client.DeleteIndex(PAGE_INDEX);
        }

        /// <summary>
        /// Creates all required indexes.
        /// </summary>
        public void EnsureIndexesCreated()
        {
            if (_client.IndexExists(PAGE_INDEX).Exists)
                return;

            _client.CreateIndex(
                PAGE_INDEX,
                m => m.Mappings(mp =>
                    mp.Map<PageDocument>(mx =>
                        mx.Properties(p =>
                            p.Text(x =>
                                x.Name(f => f.Title)
                                .Analyzer("index_ru")
                                .SearchAnalyzer("search_ru")
                            )
                            .Text(x =>
                                x.Name(f => f.Description)
                                .Analyzer("index_ru")
                                .SearchAnalyzer("search_ru")
                            )
                            .Binary(x => x.Name(f => f.Id))
                        )
                    )
                )
                .Settings(s =>
                    s.Analysis(a =>
                        a.CharFilters(c =>
                            c.Mapping("filter_ru_e", z => z.Mappings("Ё => Е", "ё => е"))
                        )
                        .Tokenizers(t =>
                            t.NGram("n_gram", ng =>
                                ng.MinGram(4).MaxGram(20)
                            )
                        )
                        .TokenFilters(t =>
                            t.Stop("stopwords_ru", st =>
                                st.StopWords(STOP_WORDS.Split(','))
                                    .IgnoreCase()
                            )
                            .WordDelimiter("delim_ru", d =>
                                d.GenerateWordParts(true)
                                    .GenerateNumberParts(true)
                                    .CatenateWords(true)
                                    .CatenateNumbers(false)
                                    .CatenateAll(true)
                                    .SplitOnCaseChange(true)
                                    .SplitOnNumerics(false)
                                    .PreserveOriginal(true)
                            )
                        )
                        .Analyzers(an =>
                            an.Custom("index_ru", ac =>
                                ac.CharFilters("html_strip", "filter_ru_e")
                                .Tokenizer("n_gram")
                                .Filters("stopwords_ru", "delim_ru", "stop", "lowercase", "russian_morphology", "english_morphology")
                            )
                            .Custom("search_ru", ac =>
                                ac.CharFilters("html_strip", "filter_ru_e")
                                .Tokenizer("standard")
                                .Filters("stopwords_ru", "delim_ru", "stop", "lowercase", "russian_morphology", "english_morphology")
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Adds a page to the index.
        /// </summary>
        public async Task AddPageAsync(Page page)
        {
            var doc = (PageDocument) page;
            await _client.IndexAsync(doc, i => i.Index(PAGE_INDEX));
        }

        /// <summary>
        /// Searches for the pages matching the query.
        /// </summary>
        public async Task<IReadOnlyList<PageDocumentSearchResult>> SearchAsync(string query)
        {
            PageDocumentSearchResult Map(IHit<PageDocument> hit)
            {
                string GetHitValue(string fieldName, string fallback)
                {
                    var value = hit.Highlights.TryGetValue(fieldName, out var hi)
                        ? hi.Highlights.FirstOrDefault()
                        : null;

                    return value ?? fallback;
                }

                return new PageDocumentSearchResult
                {
                    PageId = hit.Source.Id,
                    HighlightedTitle = GetHitValue(nameof(PageDocument.Title), hit.Source.Title),
                    HighlightedDescription = GetHitValue(nameof(PageDocument.Description), hit.Source.Description),
                };
            }

            var result = await _client.SearchAsync<PageDocument>(
                s => s.Query(q =>
                          q.MultiMatch(
                              f => f.Fields(x =>
                                        x.Fields(
                                            y => y.Title,
                                            y => y.Description
                                        )
                                    )
                                    .Query(query)
                                    .Fuzziness(Fuzziness.Auto)
                          )
                          || q.Prefix(f => f.Field(x => x.Title).Value(query))
                      )
                      .Highlight(
                          h => h.FragmentSize(200)
                                .NumberOfFragments(2)
                                .Fields(
                                    x => x.Field(f => f.Title),
                                    x => x.Field(f => f.Description)
                                )
                      )
            );

            return result.Hits.Select(Map).ToList();
        }

        #endregion
    }
}