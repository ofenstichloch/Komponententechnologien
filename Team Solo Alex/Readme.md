# C# based implementation

##Structure: 
The Broker consists of 3 elements: C# Broker, RabbitMQ, Camel instance

**C# Broker**: This program decides what message goes where. It saves a list of currently registered components and consumes all messages from the "in"-queue from the RabbitMQ Broker.

**RabbitMQ**: This is the main way of communication for the broker and the external components. The RabbitMQ broker serves 3 queues: in, out, config. There are Fanout-Exchanges for out and config.

**Camel Instance**: The Camel instance has to be written in java as we need to dynamically create new camel routes. The java program consumes messages from the config-queue and creates or deletes routes according to the URI it gets from the C# broker.
