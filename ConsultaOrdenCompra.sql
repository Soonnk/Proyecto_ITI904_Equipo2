SELECT 
c.Id AS Folio,
c.Proveedor_Id AS [Proveedor ID], 
p.Nombre AS [Nombre proveedor], 
p.Direccion AS [Dirección],
p.Telefono,
p.RFC,
m.Id AS [Id material], 
m.Nombre AS [Nombre del material], 
dtc.Cantidad, 
dtc.Costo, 
(dtc.Cantidad * dtc.Costo) AS Importe
FROM            dbo.Compras AS c INNER JOIN
                         dbo.DetalleCompras AS dtc ON c.Id = dtc.Compra_Id INNER JOIN
                         dbo.Proveedors AS p ON p.Id = c.Proveedor_Id INNER JOIN
                         dbo.Materials AS m ON m.Id = dtc.Material_Id
WHERE c.Id = 6