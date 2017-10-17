# Changes from original connector

## Inventory and price exports

The original connector contained loads of code for dealing with inventory and price updates. This was not documented anywhere, nor reflected in default connector settings.

This has been *removed*. The PIM should not be responsible for this in any way. 

## ICatalogImportHandlers implemented?

Both the original file AND the modified file will now reside in the data directory where the connector puts it's data for import.

## CVL-values

CVL-values are no longer transferred as dictionaries - only the key+value is returned (configuratble as before). Episerver has no need to maintain these values.

## Config changes

- `RESOURCE_PROVIDER_TYPE` has been removed. Nobody's going to write their own anyways.
- `EPI_MAJOR_VERSION` has been removed. It had no purpose.
- `MODIFY_FILTER_BEHAVIOR` has been removed. Invisible and undocumented, poorly named and probably never ever used.
- Support for `EPiDataType` field setting has been removed, as it's utterly pointless and can never do anything but create harm.

## Possible settings

- Setting `AllowsSearch` on your field type (with values `True` or `False`), tells the built in search index in CommerceManager whether the field is searchable or not.