using Odyssey.Models;
using System.Collections.Generic;

namespace Odyssey.Models.Documents;

public sealed record RenderedMessage(string Text, string Section, List<EntityModel> Targets);
