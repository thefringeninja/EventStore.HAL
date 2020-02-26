namespace EventStore.HAL.StreamMetadata {
	internal class StreamMetadataResource {
		public string StreamId { get; }
		public Client.StreamMetadata Metadata { get; }

		public StreamMetadataResource(string streamId, Client.StreamMetadata metadata) {
			StreamId = streamId;
			Metadata = metadata;
		}
	}
}
