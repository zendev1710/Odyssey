# Add

CTRL-D

Here you can add CRs to the one in the memory. The CR in the memory is expanded or updated with the new information. The round tag in the CR is of particular importance. A distinction is made between the following cases:

1. the CR to be added is newer than the one in the memory (new round):  
    All data on units and ships is deleted and the newer data is transferred. Region data, castles, roads, etc. are retained.  

2. the CR to be added is from the same turn as the one in memory:
    The existing unit and region data are retained and may be supplemented.  

3. the CR to be added is older than the CR in the memory:
    Only the region data is transferred - of course only if more recent data is not already available. Unfortunately, I have to say that this function does not always work 100% perfectly. In particular, merging maps that only overlap to a very limited extent usually leads to a completely incorrectly merged map. However, there is a tip: If the report to be merged is older than the one currently loaded, simply turn the tables and load the new report into the old one. Alternatively, you can also change the lap tag in the CR report itself.

Magellan automatically tries to fit matching map parts together. Magellan compares the sequence of different region types according to identical patterns. If two maps overlap, there is a good chance that Magellan will fit the maps together correctly. Problems can arise especially when regions in the added report are not clearly identifiable. Many astral regions or ocean regions, for example, are like two peas in a pod and are therefore difficult to identify.

Further information on this function can be found in the "Reference" section under [Computer reports](../../reference/cr.md).
