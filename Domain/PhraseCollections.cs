using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocabularyCards.Domain;

public class PhraseCollection
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public Language Language { get; set; }
}
