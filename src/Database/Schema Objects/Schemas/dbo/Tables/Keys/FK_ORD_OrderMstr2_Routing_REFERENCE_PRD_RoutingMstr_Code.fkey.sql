ALTER TABLE [dbo].[ORD_OrderMstr_2]
    ADD CONSTRAINT [FK_ORD_OrderMstr2_Routing_REFERENCE_PRD_RoutingMstr_Code] FOREIGN KEY ([Routing]) REFERENCES [dbo].[PRD_RoutingMstr] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

